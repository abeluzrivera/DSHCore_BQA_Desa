# DB ODS — Actualización y Modificación

**Sistema:** Data Smart Hub  
**Clasificación:** Uso Interno — Restringido  
**Responsable:** Pedro Rivera  
**Versión:** 1.0  
**Fecha:** 2026-06-16  

---

## 1. Alcance

Este documento cubre las actualizaciones sobre una `DB_ODS` ya existente y operativa. Se aplica cuando:

- Se agregan nuevas migraciones EF Core (cambios de esquema desde el código).
- Se ejecuta una recarga o enriquecimiento periódico de datos Equifax.
- Se aplican cambios manuales de esquema fuera del ciclo EF Core (hotfixes de DBA).

Para la creación desde cero, ver `db_ods_01_creacion_carga_inicial.md`.

---

## 2. Tipos de actualización

| Tipo | Frecuencia | Quién lo ejecuta | Sección |
|---|---|---|---|
| A. Migración de esquema (EF Core) | Por cada release | Desarrollador + DBA | 3 |
| B. Recarga / enriquecimiento de datos | Periódica (planificada) | DBA / SQL Agent | 4 |
| C. Hotfix de esquema (DDL manual) | Excepcional | DBA | 5 |

---

## 3. Tipo A — Migración de esquema (EF Core)

Aplica cuando hay nuevas migraciones .NET generadas con `dotnet ef migrations add`.

### 3.1 Prerequisitos

- [ ] Backup completo de `DB_ODS` tomado y verificado (`RESTORE VERIFYONLY`).
- [ ] Script de migración revisado y aprobado (ver 3.3).
- [ ] Ticket de cambio aprobado (CAB) si es Producción.
- [ ] La aplicación está detenida o en modo mantenimiento durante la ejecución.

### 3.2 Generar el script idempotente

Ejecutar desde el directorio del proyecto MVC:

```bash
# Desde CI_MVC/contactabilidad_inteligente.mcv/contactabilidad_inteligente.mcv/
dotnet ef migrations script \
  --output CI_DB/DB_ODS/Scripts/ef_migrations_vX.Y_YYYYMMDD.sql \
  --idempotent \
  -p ../../../CI_API/SDH.infrastructure \
  -s .
```

El flag `--idempotent` hace que el script detecte qué migraciones ya están aplicadas y solo ejecute las pendientes. Es seguro ejecutarlo múltiples veces.

Para generar solo las migraciones entre dos versiones específicas:

```bash
dotnet ef migrations script <MigracionDesde> <MigracionHasta> \
  -p ../../../CI_API/SDH.infrastructure \
  -s .
```

### 3.3 Revisión obligatoria del script

Antes de ejecutar en Producción, abrir el script generado y buscar:

| Señal de alerta | Riesgo | Acción |
|---|---|---|
| `DROP TABLE` | Pérdida de datos | Confirmar que la tabla está vacía o que los datos se migraron |
| `DROP COLUMN` | Pérdida de datos | Verificar que la columna no tiene datos relevantes |
| `ALTER COLUMN` que reduce longitud | Truncamiento silencioso | Verificar el tamaño máximo actual de los datos |
| `ALTER COLUMN NOT NULL` sin DEFAULT | Error en filas existentes | Confirmar que no hay NULLs en la columna |
| Renombre de tabla o columna | Ruptura de vistas, SPs y consultas externas | Revisar dependencias con `sys.sql_expression_dependencies` |

```sql
-- Verificar dependencias antes de renombrar un objeto
SELECT 
    OBJECT_NAME(referencing_id) AS ObjetoQueDepende,
    referenced_entity_name      AS ObjetoReferenciado
FROM sys.sql_expression_dependencies
WHERE referenced_entity_name = 'NombreDelObjeto';
```

### 3.4 Ejecutar la migración

**Producción (obligatorio vía script SQL):**

1. Entregar el archivo `.sql` al DBA.
2. El DBA lo ejecuta desde SSMS conectado a `DB_ODS`.
3. Verificar (sección 3.5).

**Desarrollo / QA (directo):**

```bash
dotnet ef database update -p ../../../CI_API/SDH.infrastructure -s .
```

### 3.5 Verificar

```sql
USE DB_ODS;
-- Estado actual de migraciones
SELECT MigrationId, ProductVersion FROM dbo.__EFMigrationsHistory ORDER BY MigrationId;

-- Confirmar que la última migración aparece en la tabla
-- La migración más reciente debe coincidir con el último archivo en CI_API/SDH.infrastructure/Persistence/Migrations/
```

### 3.6 Rollback de una migración EF

```bash
# Revertir a la migración anterior
dotnet ef database update <NombreMigracionAnterior> \
  -p ../../../CI_API/SDH.infrastructure -s .

# Ver el historial de migraciones disponibles
dotnet ef migrations list -p ../../../CI_API/SDH.infrastructure -s .
```

Si el rollback EF no es viable (la migración eliminó datos), restaurar desde el backup del prerequisito.

---

## 4. Tipo B — Recarga / enriquecimiento periódico de datos

Aplica para cargas periódicas desde `DB_CONTACTABILIDAD` (actualizaciones del archivo Equifax).

### 4.1 Prerequisitos

- [ ] Confirmar que los catálogos base siguen existiendo (no se eliminaron).
- [ ] Confirmar acceso a `DB_CONTACTABILIDAD` desde el servidor SQL.
- [ ] Verificar que el Job anterior finalizó correctamente (no hay ejecución en curso).
- [ ] Backup tomado si se trata de una carga de volumen muy alto.

```sql
-- Verificar que los catálogos requeridos siguen activos
USE DB_ODS;
SELECT g.Nombre_Grupo, i.Codigo_Valor, i.Id_Item_Catalogo, i.Esta_Activo
FROM parametro.Tbl_Cat_Item i
JOIN parametro.Tbl_Cat_Grupo g ON i.Id_Grupo_Catalogo = g.Id_Grupo_Catalogo
WHERE g.Nombre_Grupo IN ('ESTADO_CONTACTABILIDAD','TIPO_CONTACTO','TIPO_IDENTIFICACION','ESTADO_LOPDP')
ORDER BY g.Nombre_Grupo, i.Codigo_Valor;

-- Verificar que no hay una ejecución del Job en curso
SELECT j.name, ja.start_execution_date, ja.stop_execution_date
FROM msdb.dbo.sysjobactivity ja
JOIN msdb.dbo.sysjobs j ON ja.job_id = j.job_id
WHERE ja.start_execution_date IS NOT NULL
  AND ja.stop_execution_date IS NULL;
```

### 4.2 Comportamiento del Job en recargas

El Job `CI_DB/Jobs/Job_gration.sql` es **idempotente por diseño**:

- Los clientes que ya existen en `DB_ODS` no se insertan duplicados.
- Solo se insertan contactos y direcciones que no existan ya para ese cliente.
- Los clientes nuevos en `DB_CONTACTABILIDAD` se insertan completos.

Es seguro ejecutarlo múltiples veces sobre la misma base de datos.

### 4.3 Ejecutar

**Vía SQL Agent (recomendado):**

```sql
EXEC msdb.dbo.sp_start_job N'NombreDelJob';
```

**Monitorear progreso:**

```sql
SELECT TOP 30
    j.name,
    jh.step_name,
    jh.message,
    CASE jh.run_status
        WHEN 0 THEN 'Fallido'
        WHEN 1 THEN 'Exitoso'
        WHEN 2 THEN 'Reintentando'
        WHEN 3 THEN 'Cancelado'
        WHEN 4 THEN 'En curso'
    END AS Estado,
    CONVERT(DATETIME,
        CONVERT(VARCHAR, jh.run_date) + ' ' +
        STUFF(STUFF(RIGHT('000000' + CONVERT(VARCHAR, jh.run_time),6),5,0,':'),3,0,':')
    ) AS FechaHora
FROM msdb.dbo.sysjobhistory jh
JOIN msdb.dbo.sysjobs j ON jh.job_id = j.job_id
WHERE j.name = 'NombreDelJob'
ORDER BY jh.run_date DESC, jh.run_time DESC;
```

### 4.4 Verificar resultado

```sql
USE DB_ODS;

-- Comparar volumen contra la ejecución anterior
SELECT 'Clientes'   AS Entidad, COUNT(*) AS Total FROM operativo.Tbl_Maest_Cliente
UNION ALL SELECT 'Contactos',   COUNT(*) FROM operativo.Tbl_Contacto_Cliente
UNION ALL SELECT 'Direcciones', COUNT(*) FROM operativo.Tbl_Direccion_Cliente;

-- Clientes insertados en la última ejecución del Job
SELECT COUNT(*) AS NuevosUltimaEjecucion
FROM operativo.Tbl_Maest_Cliente
WHERE Usuario_Creacion = 'SQLJOBMigrate'
  AND Fecha_Creacion >= CAST(GETDATE() AS DATE);

-- Contactos enriquecidos en la última ejecución
SELECT COUNT(*) AS ContactosNuevosUltimaEjecucion
FROM operativo.Tbl_Contacto_Cliente
WHERE Usuario_Creacion = 'SQLJOBMigrate'
  AND Fecha_Creacion >= CAST(GETDATE() AS DATE);
```

### 4.5 Rollback de una carga

El Job no tiene rollback automático a nivel de datos ya confirmados (cada lote hace `COMMIT` independiente). Si se necesita deshacer una carga completa, eliminar los registros insertados por esa ejecución:

```sql
USE DB_ODS;

-- Definir la fecha y hora exacta de inicio de la carga a revertir
DECLARE @FechaEjecucion DATETIME2 = '2026-06-16 14:00:00';

BEGIN TRAN;

DELETE FROM operativo.Tbl_Direccion_Cliente
WHERE Usuario_Creacion = 'SQLJOBMigrate' AND Fecha_Creacion >= @FechaEjecucion;

DELETE FROM operativo.Tbl_Contacto_Cliente
WHERE Usuario_Creacion = 'SQLJOBMigrate' AND Fecha_Creacion >= @FechaEjecucion;

DELETE FROM operativo.Tbl_Maest_Cliente
WHERE Usuario_Creacion = 'SQLJOBMigrate' AND Fecha_Creacion >= @FechaEjecucion;

-- Revisar conteos antes de confirmar
SELECT @@ROWCOUNT;
-- COMMIT TRAN; -- descomentar solo si los conteos son correctos
-- ROLLBACK TRAN; -- usar si los conteos no corresponden
```

---

## 5. Tipo C — Hotfix de esquema (DDL manual)

Solo para casos excepcionales donde un cambio de esquema no puede esperar al ciclo normal de EF Core. Requiere aprobación del equipo de desarrollo antes de ejecutar.

### 5.1 Prerequisitos

- [ ] Aprobación escrita del responsable técnico del proyecto.
- [ ] Backup completo verificado.
- [ ] Script DDL revisado por al menos otro miembro del equipo.
- [ ] El cambio está documentado para que la siguiente migración EF lo refleje.

### 5.2 Reglas para hotfixes DDL

- **Solo operaciones aditivas sin riesgo:** `ADD COLUMN`, `CREATE INDEX`, `CREATE VIEW`.
- **Nunca sin transacción explícita** si el cambio es reversible.
- **El cambio debe registrarse en `__EFMigrationsHistory`** si EF Core necesita conocerlo, o la siguiente migración debe generarse como `--idempotent` para no volver a aplicarlo.

### 5.3 Plantilla de script de hotfix

```sql
-- Hotfix DDL — DB_ODS
-- Ticket: [número de ticket]
-- Fecha: [fecha]
-- Autor: [nombre]
-- Revisado por: [nombre]
-- Descripción: [descripción del cambio]

USE DB_ODS;
GO

SET XACT_ABORT ON;
BEGIN TRAN;

-- [DDL aquí]
-- Ejemplo:
-- ALTER TABLE operativo.Tbl_Maest_Cliente ADD Nueva_Columna NVARCHAR(50) NULL;

-- Verificar antes de confirmar
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Tbl_Maest_Cliente';

COMMIT TRAN;
-- En caso de error: ROLLBACK TRAN;
```

### 5.4 Post-hotfix

Notificar al desarrollador responsable para que genere una migración EF vacía que marque el cambio como conocido por EF Core, evitando conflictos en el próximo despliegue:

```bash
# Crear una migración vacía que registre el estado actual
dotnet ef migrations add Hotfix_NombreDelCambio \
  -p ../../../CI_API/SDH.infrastructure -s .
# Luego editar la migración generada para que refleje el cambio real
```

---

## 6. Consideraciones de seguridad

### Mínimo privilegio

En cada actualización, verificar que `usr_ods` no escaló a `db_owner`:

```sql
USE DB_ODS;
SELECT dp.name AS Usuario, dp2.name AS Rol
FROM sys.database_role_members drm
JOIN sys.database_principals dp  ON drm.member_principal_id = dp.principal_id
JOIN sys.database_principals dp2 ON drm.role_principal_id   = dp2.principal_id
WHERE dp.name = 'usr_ods';
```

### Auditoría de cambios

Todos los cambios de esquema en Producción deben quedar registrados en la bitácora de cambios del banco con:

- Número de ticket CAB.
- Hash o nombre del script ejecutado.
- Usuario que ejecutó.
- Fecha y hora exacta.
- Resultado (exitoso / fallido).

### LOPDP

Los datos cargados por el Job ingresan con `Id_Estado_LOPDP = PEND`. Ningún proceso de negocio debe tratar esos registros como habilitados para contacto hasta que pasen por el flujo de validación de la aplicación.

---

## 7. Registro de ejecuciones

Llenar por cada actualización ejecutada.

| Campo | Valor |
|---|---|
| Fecha y hora de inicio | |
| Fecha y hora de fin | |
| Tipo de actualización (A / B / C) | |
| Ambiente | |
| Ejecutor (usuario AD) | |
| Número de ticket CAB | |
| Descripción del cambio | |
| Resultado | Exitoso / Fallido |
| Observaciones | |
