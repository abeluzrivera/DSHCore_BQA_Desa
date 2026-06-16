# DB ODS — Creación y Carga Inicial

**Sistema:** Data Smart Hub  
**Clasificación:** Uso Interno — Restringido  
**Responsable:** Pedro Rivera  
**Versión:** 1.0  
**Fecha:** 2026-06-16  

---

## 1. Alcance

Este documento cubre el proceso de creación desde cero de la base de datos `DB_ODS` y la carga inicial de datos. Se ejecuta **una sola vez por ambiente**.

Para actualizaciones incrementales posteriores, ver `db_ods_02_actualizacion.md`.

La creación cubre tres capas que deben ejecutarse en orden estricto:

| # | Capa | Responsable | Artefacto |
|---|---|---|---|
| 1 | Estructura de base de datos | DBA | `CI_DB/DB_ODS/Scripts/DB_ODS_1.0.0.sql` |
| 2 | Esquema de aplicación (EF Core) | Desarrollador | `dotnet ef migrations script --idempotent` |
| 3 | Carga masiva de datos (Equifax) | DBA / SQL Agent | `CI_DB/Jobs/Job_gration.sql` |

---

## 2. Ambientes

| Ambiente | Base de datos | Observaciones |
|---|---|---|
| Desarrollo | `DB_ODS_DEV` | Puede recrearse sin CAB |
| Certificación / QA | `DB_ODS_DEV` | Requiere notificación previa |
| Producción | `DB_ODS` | Requiere aprobación CAB |

---

## 3. Prerequisitos

Verificar antes de iniciar. No avanzar si algún punto falla.

### 3.1 Aprobaciones (solo Producción)

- [ ] Ticket de cambio aprobado por el Comité de Cambios (CAB).
- [ ] Ventana de mantenimiento acordada y notificada a los usuarios.
- [ ] Autorización del Oficial de Seguridad de la Información (OSI).
- [ ] Notificación enviada a Continuidad de Negocio.

### 3.2 Backup previo (si la BD ya existe)

- [ ] Backup completo tomado y verificado con `RESTORE VERIFYONLY`.
- [ ] Ubicación del backup registrada en la bitácora de cambios.

```sql
-- Verificar que el backup es válido antes de proceder
RESTORE VERIFYONLY FROM DISK = 'RUTA\DB_ODS_pre_creacion.bak';
```

### 3.3 Conectividad y permisos

- [ ] El ejecutor tiene permisos `sysadmin` o `dbcreator` en `[master]`.
- [ ] El login `usr_ods` existe a nivel de servidor SQL.
- [ ] La cuenta del SQL Agent tiene acceso a `DB_CONTACTABILIDAD`.
- [ ] Las rutas de archivos del servidor son accesibles (`E:\SQL\Data\`, `L:\SQL\Log\`).

```sql
-- Verificar login de aplicación
SELECT name, type_desc FROM sys.server_principals WHERE name = 'usr_ods';

-- Verificar acceso a la fuente de datos
SELECT name FROM sys.servers WHERE is_linked = 1;
```

---

## 4. Capa 1 — Crear la estructura de base de datos

**Script:** `CI_DB/DB_ODS/Scripts/DB_ODS_1.0.0.sql`  
**Conectar a:** instancia SQL Server, base de datos `[master]`  
**Ejecutor:** DBA con permisos `sysadmin`

### 4.1 Qué crea el script

- Base de datos `DB_ODS` con archivos en `E:\SQL\Data\` y `L:\SQL\Log\`.
- Schemas: `operativo`, `parametro`, `seguridad`.
- 7 tablas, 2 vistas, todos los índices y foreign keys.
- Usuario `usr_ods` mapeado al login del mismo nombre.

> El script es **destructivo**: elimina `DB_ODS` si existe y la recrea. El backup del paso 3.2 es el único resguardo.

### 4.2 Correcciones de seguridad obligatorias antes de ejecutar

**Problema 1 — Privilegio excesivo en `usr_ods`:**

El script original asigna `db_owner`. Esto viola el principio de mínimo privilegio. Editar el script y reemplazar:

```sql
-- ELIMINAR esta línea:
ALTER ROLE [db_owner] ADD MEMBER [usr_ods]

-- MANTENER solo estas dos:
ALTER ROLE [db_datareader] ADD MEMBER [usr_ods]
ALTER ROLE [db_datawriter] ADD MEMBER [usr_ods]
-- Si la aplicación usa stored procedures, agregar también:
-- GRANT EXECUTE TO [usr_ods]
```

**Problema 2 — Propietario de base de datos incorrecto:**

Eliminar esta línea del script antes de ejecutar en Producción:

```sql
-- ELIMINAR en Producción:
ALTER AUTHORIZATION ON DATABASE::[DB_ODS] TO [BANCOMACHALA\pedro.rivera]
```

El propietario en Producción debe ser `sa` o la cuenta de servicio definida por el DBA.

**Problema 3 — Rutas de archivos:**

Verificar que las rutas correspondan al servidor objetivo:

```sql
-- Ajustar según el servidor si es necesario:
FILENAME = N'E:\SQL\Data\DB_ODS.mdf'
FILENAME = N'L:\SQL\Log\DB_ODS_log.ldf'
```

### 4.3 Pasos

1. Abrir SSMS conectado a la instancia objetivo.
2. Abrir `CI_DB/DB_ODS/Scripts/DB_ODS_1.0.0.sql`.
3. Aplicar las tres correcciones del punto 4.2.
4. Ejecutar el script completo.
5. Verificar:

```sql
USE DB_ODS;
-- Tablas creadas
SELECT TABLE_SCHEMA, TABLE_NAME FROM INFORMATION_SCHEMA.TABLES ORDER BY 1, 2;
-- Schemas creados
SELECT name FROM sys.schemas WHERE name IN ('operativo','parametro','seguridad');
-- Foreign keys activas (debe retornar 0 filas)
SELECT name FROM sys.foreign_keys WHERE is_disabled = 1;
```

**Resultado esperado:** 7 tablas, 3 schemas, 2 vistas, 0 foreign keys deshabilitadas.

---

## 5. Capa 2 — Migraciones EF Core

**Herramienta:** `dotnet ef`  
**Directorio:** `CI_MVC/contactabilidad_inteligente.mcv/contactabilidad_inteligente.mcv/`  
**Ejecutor:** Desarrollador

### 5.1 Qué hace esta capa

Crea la tabla `[dbo].[__EFMigrationsHistory]` y aplica todos los cambios de esquema gestionados por el código .NET desde el inicio del proyecto. Es idempotente.

### 5.2 Generar el script de migración

```bash
# Desde el directorio del proyecto MVC
dotnet ef migrations script \
  --output CI_DB/DB_ODS/Scripts/ef_migrations_inicial.sql \
  --idempotent \
  -p ../../../CI_API/SDH.infrastructure \
  -s .
```

### 5.3 Revisión del script (obligatorio para Producción)

Antes de ejecutar, abrir el script generado y verificar que no contiene:

- `DROP TABLE` o `DROP COLUMN` inesperados.
- `ALTER COLUMN` que reduzca longitud o cambie tipo de dato.
- Renombres de objetos sin migración de datos asociada.

### 5.4 Ejecutar

**Producción (recomendado):** ejecutar el script `.sql` generado desde SSMS como DBA.

**Desarrollo / QA (directo):**

```bash
dotnet ef database update -p ../../../CI_API/SDH.infrastructure -s .
```

### 5.5 Verificar

```sql
USE DB_ODS;
SELECT MigrationId, ProductVersion FROM dbo.__EFMigrationsHistory ORDER BY MigrationId;
```

Debe retornar todas las migraciones del proyecto sin errores.

---

## 6. Capa 3 — Carga masiva de datos (Job Equifax)

**Script:** `CI_DB/Jobs/Job_gration.sql`  
**Fuente:** `DB_CONTACTABILIDAD` (Equifax — contactabilidad + direcciones + geolocalización)  
**Destino:** `DB_ODS`, schemas `operativo` y `parametro`

### 6.1 Prerequisito: seed de catálogos

El Job valida la existencia de catálogos al inicio. Si no existen, aborta con error. Deben estar cargados antes de ejecutar el Job.

Grupos y códigos requeridos:

| Grupo | Código | Descripción |
|---|---|---|
| `ESTADO_CONTACTABILIDAD` | `PEND` | Estado pendiente de verificación |
| `TIPO_CONTACTO` | `CEL` | Celular |
| `TIPO_CONTACTO` | `TEL_C` | Teléfono convencional |
| `TIPO_CONTACTO` | `EMAIL` | Correo electrónico |
| `TIPO_CONTACTO` | `DIR_D` | Dirección domicilio |
| `TIPO_IDENTIFICACION` | `201` | Cédula de identidad |
| `ESTADO_LOPDP` | `6` | Pendiente LOPDP |

El seed lo aplica la aplicación al iniciar en modo `Development`, o mediante el script de datos iniciales para Producción.

Verificar antes de continuar:

```sql
USE DB_ODS;
SELECT g.Nombre_Grupo, i.Codigo_Valor, i.Id_Item_Catalogo
FROM parametro.Tbl_Cat_Item i
JOIN parametro.Tbl_Cat_Grupo g ON i.Id_Grupo_Catalogo = g.Id_Grupo_Catalogo
WHERE g.Nombre_Grupo IN ('ESTADO_CONTACTABILIDAD','TIPO_CONTACTO','TIPO_IDENTIFICACION','ESTADO_LOPDP')
ORDER BY g.Nombre_Grupo, i.Codigo_Valor;
```

### 6.2 Qué hace el Job

```
DB_CONTACTABILIDAD
  ├── Tbl_Contactabilidad       (teléfonos / emails Equifax)
  ├── tbl_maestrodirecciones    (direcciones Equifax)
  └── ScoreMiDireccion          (geolocalización)
         ↓  JOIN + FULL OUTER JOIN
    #MigracionRaw  (TempDB)
         ↓  deduplicación por Identificacion
    ┌── Clientes ya en DB_ODS → enriquecimiento de contactos y direcciones
    └── Clientes nuevos → inserción en lotes de 50.000
              ↓
         Tbl_Maest_Cliente + Tbl_Contacto_Cliente + Tbl_Direccion_Cliente
```

Todos los contactos se insertan con `Id_Estado_LOPDP = PEND` — no están habilitados para contacto activo hasta validación posterior, en cumplimiento de la LOPDP.

Si el Job se interrumpe, puede reiniciarse sin riesgo. Los registros ya procesados se detectan como existentes y solo se enriquecen.

### 6.3 Ejecutar el Job

**SQL Server Agent (recomendado para Producción):**

```sql
-- Iniciar el job
EXEC msdb.dbo.sp_start_job N'NombreDelJob';

-- Monitorear progreso en tiempo real
SELECT TOP 20
    j.name,
    jh.step_name,
    jh.message,
    jh.run_status,
    CONVERT(DATETIME,
        CONVERT(VARCHAR, jh.run_date) + ' ' +
        STUFF(STUFF(RIGHT('000000' + CONVERT(VARCHAR, jh.run_time),6),5,0,':'),3,0,':')
    ) AS FechaHora
FROM msdb.dbo.sysjobhistory jh
JOIN msdb.dbo.sysjobs j ON jh.job_id = j.job_id
WHERE j.name = 'NombreDelJob'
ORDER BY jh.run_date DESC, jh.run_time DESC;
```

**Ejecución directa en SSMS (solo Desarrollo / QA):**

Abrir `CI_DB/Jobs/Job_gration.sql` en SSMS, conectado a la instancia que tiene acceso a `DB_CONTACTABILIDAD`, y ejecutar. El progreso se imprime por lote con `RAISERROR ... WITH NOWAIT`.

---

## 7. Validación final

### 7.1 Volumen de datos

```sql
USE DB_ODS;
SELECT 'Clientes'         AS Entidad, COUNT(*) AS Total FROM operativo.Tbl_Maest_Cliente
UNION ALL SELECT 'Contactos',          COUNT(*) FROM operativo.Tbl_Contacto_Cliente
UNION ALL SELECT 'Direcciones',        COUNT(*) FROM operativo.Tbl_Direccion_Cliente
UNION ALL SELECT 'Grupos catálogo',    COUNT(*) FROM parametro.Tbl_Cat_Grupo
UNION ALL SELECT 'Items catálogo',     COUNT(*) FROM parametro.Tbl_Cat_Item
UNION ALL SELECT 'Usuarios sistema',   COUNT(*) FROM seguridad.Tbl_Maest_Usuario;
```

### 7.2 Calidad de datos

```sql
USE DB_ODS;

-- Clientes sin contactos (puede indicar fallo en la carga)
SELECT COUNT(*) AS ClientesSinContactos
FROM operativo.Tbl_Maest_Cliente c
WHERE NOT EXISTS (
    SELECT 1 FROM operativo.Tbl_Contacto_Cliente cc
    WHERE cc.Id_Cliente = c.Id_Cliente AND cc.Esta_Eliminado = 0
) AND c.Esta_Eliminado = 0;

-- Contactos con valor vacío (no deben existir)
SELECT COUNT(*) AS ContactosVacios
FROM operativo.Tbl_Contacto_Cliente
WHERE LTRIM(RTRIM(Valor_Contacto)) = '' OR Valor_Contacto IS NULL;

-- Distribución por fuente de datos
SELECT Source, Id_Tipo_Contacto, COUNT(*) AS Total
FROM operativo.Tbl_Contacto_Cliente
GROUP BY Source, Id_Tipo_Contacto
ORDER BY Source, Id_Tipo_Contacto;
```

### 7.3 Permisos del usuario de aplicación

```sql
USE DB_ODS;
-- En Producción debe mostrar solo db_datareader y db_datawriter
SELECT dp.name AS Usuario, dp2.name AS Rol
FROM sys.database_role_members drm
JOIN sys.database_principals dp  ON drm.member_principal_id = dp.principal_id
JOIN sys.database_principals dp2 ON drm.role_principal_id   = dp2.principal_id
WHERE dp.name = 'usr_ods';
```

Si aparece `db_owner`, revocar:

```sql
ALTER ROLE [db_owner] DROP MEMBER [usr_ods];
```

### 7.4 Smoke test de aplicación

1. Iniciar la aplicación apuntando a `DB_ODS`.
2. Iniciar sesión con un usuario del schema `seguridad`.
3. Buscar un cliente por cédula y confirmar que devuelve datos.
4. Verificar que la vista funciona:

```sql
SELECT TOP 10 * FROM operativo.vw_Clientes_Contactos_Detalle;
```

---

## 8. Plan de rollback

### Si falla la Capa 1 (estructura)

```sql
USE master;
ALTER DATABASE DB_ODS SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
RESTORE DATABASE DB_ODS FROM DISK = 'RUTA\DB_ODS_pre_creacion.bak'
    WITH REPLACE, RECOVERY;
ALTER DATABASE DB_ODS SET MULTI_USER;
```

### Si falla la Capa 2 (migraciones EF)

```bash
# Revertir a la migración anterior conocida
dotnet ef database update <NombreMigracionAnterior> \
  -p ../../../CI_API/SDH.infrastructure -s .
```

Si no es posible revertir, restaurar desde backup (igual que Capa 1).

### Si falla la Capa 3 (Job de datos)

El Job usa transacciones por lote. No deja datos parciales. Puede reiniciarse desde el inicio. Para un rollback total a estado vacío:

```sql
USE DB_ODS;
-- En orden inverso de dependencias
DELETE FROM operativo.Tbl_Oficializacion_Core;
DELETE FROM operativo.Tbl_Direccion_Cliente;
DELETE FROM operativo.Tbl_Contacto_Cliente;
DELETE FROM operativo.Tbl_Financiero_cliente;
DELETE FROM operativo.Tbl_Maest_Cliente;
-- Resetear identidades
DBCC CHECKIDENT ('operativo.Tbl_Maest_Cliente',    RESEED, 0);
DBCC CHECKIDENT ('operativo.Tbl_Contacto_Cliente', RESEED, 0);
DBCC CHECKIDENT ('operativo.Tbl_Direccion_Cliente',RESEED, 0);
DBCC CHECKIDENT ('operativo.Tbl_Financiero_cliente',RESEED, 0);
```

---

## 9. Problemas conocidos

| Problema | Causa probable | Solución |
|---|---|---|
| `Fallo de Integridad: No se encontraron los catálogos base` | Seed no ejecutado | Ejecutar seed antes del Job (sección 6.1) |
| `Cannot open database DB_CONTACTABILIDAD` | Sin acceso al servidor fuente | Verificar linked server o ejecutar en la instancia correcta |
| `usr_ods` no puede conectarse | Login no existe en el servidor | `CREATE LOGIN [usr_ods] WITH PASSWORD = '...'` |
| Migraciones EF fallan con `Cannot find object '__EFMigrationsHistory'` | Capa 1 no ejecutada | Ejecutar `DB_ODS_1.0.0.sql` primero |
| El archivo `.mdf` no se puede crear | La ruta no existe en el servidor | Crear la carpeta o ajustar la ruta en el script |

---

## 10. Registro de ejecución

| Campo | Valor |
|---|---|
| Fecha y hora de inicio | |
| Fecha y hora de fin | |
| Ambiente | |
| Ejecutor (usuario AD) | |
| Número de ticket CAB | |
| Clientes insertados (Capa 3) | |
| Clientes enriquecidos (Capa 3) | |
| Resultado | Exitoso / Fallido |
| Observaciones | |
