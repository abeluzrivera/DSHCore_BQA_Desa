# Pase a Producción — Alcance 2026-054

**Sistema:** Contactabilidad Inteligente — Data Smart Hub
**Ambiente destino:** Homologación / Producción
**Fecha de emisión:** 2026-07-06
**Responsable técnico:** Pedro Abel

---

## Tabla de Contenidos

1. [Descripción del alcance](#1-descripción-del-alcance)
2. [Detener el sitio en IIS](#2-detener-el-sitio-en-iis)
3. [Respaldar archivos actuales](#3-respaldar-archivos-actuales)
4. [Desplegar los binarios compilados](#4-desplegar-los-binarios-compilados)
5. [Verificar appsettings.json](#5-verificar-appssettingsjson)
6. [Ejecutar scripts de base de datos](#6-ejecutar-scripts-de-base-de-datos)
7. [Iniciar el sitio en IIS](#7-iniciar-el-sitio-en-iis)
8. [Verificación post-pase](#8-verificación-post-pase)
9. [Plan de rollback](#9-plan-de-rollback)

---

## 1. Descripción del alcance

Este pase implementa el módulo de **Reporte Diario de Actividad de Contactabilidad**. Genera diariamente un registro persistente de todos los contactos y direcciones confirmados o agregados por asesor, con automatización vía SQL Server Agent y consumo desde Power BI para presentación a alta gerencia.

### Cambios incluidos

| Componente | Tipo de cambio | Descripción |
|---|---|---|
| Binarios de la aplicación | Actualización | DLL compilados con correcciones de este alcance |
| `Program.cs` | Corrección | Reemplazo de `BuildServiceProvider()` en bloques catch — elimina warning ASP0000 |
| `Program.cs` | Mejora | Log del puerto al iniciar la aplicación |
| `Users.cs` | Corrección | Guard para `PasswordHash` vacío antes de invocar BCrypt |
| DB — Schema `reportes` | Nuevo | Schema nuevo con 4 tablas de reportería |
| DB — Stored Procedures | Nuevo | `SP_Baseline_Carga_Inicial` y `SP_Reporte_Actividad_Diaria` |
| DB — SQL Agent Jobs | Nuevo | `JOB_Baseline_Carga_Inicial` y `JOB_Reporte_Actividad_Diaria` |

### Archivos entregados

| Archivo | Contenido |
|---|---|
| `datahub_alcance_54.zip` | Binarios compilados de la aplicación |
| `CI_DB/DB_ODS/Scripts/02_Schema_Tablas_Reportes.sql` | DDL del schema y tablas |
| `CI_DB/DB_ODS/Scripts/03_Backfill_Actividad_Historica.sql` | Carga histórica de datos previos al job |
| `CI_DB/DB_ODS/SP/SP_Baseline_Carga_Inicial.sql` | Stored procedure de baseline |
| `CI_DB/DB_ODS/SP/SP_Reporte_Actividad_Diaria.sql` | Stored procedure nocturno |
| `CI_DB/Jobs/Job_Baseline_Carga_Inicial.sql` | SQL Agent Job de baseline |
| `CI_DB/Jobs/Job_Reporte_Actividad_Diaria.sql` | SQL Agent Job nocturno |

---

## 2. Detener el sitio en IIS

Detenga el sitio antes de reemplazar los binarios para evitar archivos bloqueados.

1. Abra el **Administrador de IIS** (`inetmgr`).
2. En el panel de conexiones expanda **"Sitios"**.
3. Clic derecho sobre **`Data Smart Hub`** → **"Detener"**.

Alternativamente desde PowerShell con privilegios de administrador:

```powershell
Stop-WebSite -Name "Data Smart Hub"
Stop-WebAppPool -Name "SmartHubPool"
```

> Confirme que el sitio quedó en estado **"Detenido"** antes de continuar.

---

## 3. Respaldar archivos actuales

> **Este paso es obligatorio.** El respaldo permite ejecutar el rollback sin pérdida de datos en caso de falla.

### 3.1 Respaldar appsettings.json

```powershell
$fecha = Get-Date -Format "yyyyMMdd"
Copy-Item "C:\Sitios\DataSmartHub\appsettings.json" `
          "C:\Sitios\DataSmartHub\appsettings.json.bak_$fecha"
```

Confirme que el archivo de respaldo fue creado:

```powershell
Get-Item "C:\Sitios\DataSmartHub\appsettings.json.bak_*" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
```

### 3.2 Respaldar binarios actuales

```powershell
$fecha = Get-Date -Format "yyyyMMdd"
Copy-Item "C:\Sitios\DataSmartHub\" `
          "C:\Respaldos\DataSmartHub_$fecha\" -Recurse
```

---

## 4. Desplegar los binarios compilados

### 4.1 Extraer el paquete

1. Ubique el archivo `datahub_alcance_54.zip` en el servidor.
2. Extraiga el contenido sobre la carpeta del sitio:

   ```
   C:\Sitios\DataSmartHub\
   ```

3. Cuando solicite confirmación para reemplazar archivos existentes, seleccione **"Reemplazar todos"**.

> **No elimine** el archivo `appsettings.json` durante este paso. El paquete no incluye ese archivo.

### 4.2 Verificar la estructura post-extracción

```
C:\Sitios\DataSmartHub\
├── appsettings.json               ← no fue reemplazado
├── contactabilidad_inteligente.mcv.dll
├── SDH.Application.dll
├── SDH.Domain.dll
├── SDH.infrastructure.dll
├── wwwroot\
└── [demás DLL de dependencias]
```

---

## 5. Verificar appsettings.json

Este alcance **no requiere cambios** en `appsettings.json`. Sin embargo, verifique que el archivo de respaldo del paso 3.1 existe y que el archivo activo es válido antes de continuar:

```powershell
Get-Content "C:\Sitios\DataSmartHub\appsettings.json" | ConvertFrom-Json
# Si no lanza error, el JSON es válido
```

> Si en algún alcance futuro se deban agregar claves nuevas al appsettings, se documentará en esta sección con el bloque JSON exacto a insertar y la ubicación dentro del archivo.

---

## 6. Ejecutar scripts de base de datos

> Ejecutar en **SSMS** en el orden indicado. Cada script incluye su query de verificación al final.

### Prerequisitos

- SQL Server Agent habilitado en la instancia destino
- Conexión con usuario con permisos `db_owner` en `DB_ODS`
- Fecha de migración inicial: **2026-06-24** (se detecta automáticamente)

---

### Paso 6.1 — Crear schema y tablas

**Script:** `CI_DB/DB_ODS/Scripts/02_Schema_Tablas_Reportes.sql`
**Base de datos:** `DB_ODS`
**Idempotente:** Sí

Ejecute el script completo. Al finalizar, el script retorna la verificación automáticamente:

```sql
-- Resultado esperado: 4 filas
-- Tbl_Baseline_Carga_Inicial
-- Tbl_Det_Actividad_Diaria
-- Tbl_Log_Jobs
-- Tbl_Sum_Actividad_Diaria
```

---

### Paso 6.2 — Crear SP de baseline

**Script:** `CI_DB/DB_ODS/SP/SP_Baseline_Carga_Inicial.sql`
**Base de datos:** `DB_ODS`
**Idempotente:** Sí (`CREATE OR ALTER`)

Verificación:

```sql
SELECT OBJECT_ID('reportes.SP_Baseline_Carga_Inicial');
-- Resultado esperado: valor distinto de NULL
```

---

### Paso 6.3 — Crear SP de reporte diario

**Script:** `CI_DB/DB_ODS/SP/SP_Reporte_Actividad_Diaria.sql`
**Base de datos:** `DB_ODS`
**Idempotente:** Sí (`CREATE OR ALTER`)

Verificación:

```sql
SELECT OBJECT_ID('reportes.SP_Reporte_Actividad_Diaria');
-- Resultado esperado: valor distinto de NULL
```

---

### Paso 6.4 — Ejecutar SP de baseline (única vez)

**Acción manual en SSMS:**

```sql
USE [DB_ODS];
EXEC reportes.SP_Baseline_Carga_Inicial;
```

Verificación:

```sql
SELECT * FROM reportes.Tbl_Baseline_Carga_Inicial;
SELECT * FROM reportes.Tbl_Log_Jobs;
-- Resultado esperado:
--   Tbl_Baseline_Carga_Inicial: 1 fila con Fecha_Migracion = '2026-06-24'
--   Tbl_Log_Jobs: 1 fila con Estado = 'EXITOSO'
```

> **Importante:** Si se ejecuta más de una vez retorna un error controlado y no duplica datos. Es seguro re-ejecutar.

---

### Paso 6.5 — Backfill de datos históricos (única vez)

**Script:** `CI_DB/DB_ODS/Scripts/03_Backfill_Actividad_Historica.sql`
**Base de datos:** `DB_ODS`
**Idempotente:** Sí

Procesa todos los días desde **2026-06-25** hasta el día anterior a la ejecución. Al finalizar, el script retorna la verificación automáticamente:

```sql
-- Resultado esperado: una fila por cada día con actividad entre 2026-06-25 y ayer
-- Los días sin actividad registrada no aparecerán (es correcto)
```

---

### Paso 6.6 — Crear SQL Agent Job de baseline

**Script:** `CI_DB/Jobs/Job_Baseline_Carga_Inicial.sql`
**Base de datos:** `msdb`
**Idempotente:** Sí

Verificación:

```sql
SELECT name, enabled FROM msdb.dbo.sysjobs
WHERE name = 'JOB_Baseline_Carga_Inicial';
-- Resultado esperado: 1 fila con enabled = 1
```

---

### Paso 6.7 — Crear SQL Agent Job nocturno

**Script:** `CI_DB/Jobs/Job_Reporte_Actividad_Diaria.sql`
**Base de datos:** `msdb`
**Idempotente:** Sí
**Schedule:** Diario a las 01:00 AM

Verificación:

```sql
SELECT j.name, j.enabled, s.active_start_time
FROM msdb.dbo.sysjobs j
JOIN msdb.dbo.sysjobschedules js ON js.job_id = j.job_id
JOIN msdb.dbo.sysschedules s ON s.schedule_id = js.schedule_id
WHERE j.name = 'JOB_Reporte_Actividad_Diaria';
-- Resultado esperado: 1 fila con enabled = 1, active_start_time = 10000 (01:00 AM)
```

---

### Resumen de scripts

| Paso | Script | BD | Única vez |
|---|---|---|---|
| 6.1 | `02_Schema_Tablas_Reportes.sql` | DB_ODS | No |
| 6.2 | `SP_Baseline_Carga_Inicial.sql` | DB_ODS | No |
| 6.3 | `SP_Reporte_Actividad_Diaria.sql` | DB_ODS | No |
| 6.4 | `EXEC reportes.SP_Baseline_Carga_Inicial` | DB_ODS | **Sí** |
| 6.5 | `03_Backfill_Actividad_Historica.sql` | DB_ODS | **Sí** |
| 6.6 | `Job_Baseline_Carga_Inicial.sql` | msdb | No |
| 6.7 | `Job_Reporte_Actividad_Diaria.sql` | msdb | No |

---

## 7. Iniciar el sitio en IIS

### 7.1 Iniciar el sitio y el pool

```powershell
Start-WebAppPool -Name "SmartHubPool"
Start-WebSite -Name "Data Smart Hub"
```

### 7.2 Verificar estado

```powershell
Get-WebSite -Name "Data Smart Hub" | Select-Object Name, State
Get-WebAppPoolState -Name "SmartHubPool"
# Ambos deben mostrar: Started
```

---

## 8. Verificación post-pase

### 8.1 Funcional

- [ ] El sitio responde en el puerto configurado.
- [ ] El login con usuario de base de datos funciona correctamente.
- [ ] El dashboard carga sin errores.

### 8.2 Base de datos — schema reportes

```sql
USE DB_ODS;

-- Tablas creadas
SELECT s.name AS schema_name, t.name AS tabla
FROM sys.tables t
JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE s.name = 'reportes'
ORDER BY t.name;
-- Resultado esperado: 4 filas

-- Baseline ejecutado
SELECT Fecha_Migracion, Total_Contactos, Total_Direcciones
FROM reportes.Tbl_Baseline_Carga_Inicial;
-- Resultado esperado: 1 fila con Fecha_Migracion = '2026-06-24'

-- Backfill ejecutado
SELECT Fecha_Reporte, SUM(Total_Registros) AS Total
FROM reportes.Tbl_Sum_Actividad_Diaria
GROUP BY Fecha_Reporte
ORDER BY Fecha_Reporte;
-- Resultado esperado: filas desde 2026-06-25 hasta ayer
```

### 8.3 SQL Agent Jobs

- [ ] `JOB_Baseline_Carga_Inicial` existe y está habilitado.
- [ ] `JOB_Reporte_Actividad_Diaria` existe, habilitado y con schedule a las 01:00 AM.

### 8.4 Logs de aplicación

```powershell
Get-Content "C:\Sitios\DataSmartHub\Logs\log-*.txt" -Tail 30 | Select-String "Aplicacion iniciada|Error|Critical"
# Debe aparecer "Aplicacion iniciada. Escuchando en: https://..." sin errores críticos
```

---

## 9. Plan de rollback

### 9.1 Detener el sitio

```powershell
Stop-WebSite -Name "Data Smart Hub"
Stop-WebAppPool -Name "SmartHubPool"
```

### 9.2 Restaurar binarios

```powershell
$fecha = "20260706"   # ajustar a la fecha del respaldo generado en paso 3.2
Get-ChildItem "C:\Sitios\DataSmartHub\" -Exclude "appsettings.json" | Remove-Item -Recurse -Force
Copy-Item "C:\Respaldos\DataSmartHub_$fecha\*" `
          "C:\Sitios\DataSmartHub\" -Recurse -Force
```

### 9.3 Restaurar appsettings.json

```powershell
$fecha = "20260706"   # ajustar a la fecha del respaldo generado en paso 3.1
Copy-Item "C:\Sitios\DataSmartHub\appsettings.json.bak_$fecha" `
          "C:\Sitios\DataSmartHub\appsettings.json" -Force
```

### 9.4 Revertir base de datos

```sql
USE [msdb];
EXEC msdb.dbo.sp_delete_job @job_name = N'JOB_Reporte_Actividad_Diaria', @delete_unused_schedule = 1;
EXEC msdb.dbo.sp_delete_job @job_name = N'JOB_Baseline_Carga_Inicial',   @delete_unused_schedule = 1;

USE [DB_ODS];
DROP PROCEDURE IF EXISTS reportes.SP_Reporte_Actividad_Diaria;
DROP PROCEDURE IF EXISTS reportes.SP_Baseline_Carga_Inicial;
DROP TABLE IF EXISTS reportes.Tbl_Sum_Actividad_Diaria;
DROP TABLE IF EXISTS reportes.Tbl_Det_Actividad_Diaria;
DROP TABLE IF EXISTS reportes.Tbl_Baseline_Carga_Inicial;
DROP TABLE IF EXISTS reportes.Tbl_Log_Jobs;
DROP SCHEMA IF EXISTS reportes;
```

### 9.5 Reiniciar el sitio

```powershell
Start-WebAppPool -Name "SmartHubPool"
Start-WebSite -Name "Data Smart Hub"
```

---

*Documento generado para el pase del alcance 2026-054.*
*Para consultas técnicas, contacte al responsable indicado en la cabecera.*
