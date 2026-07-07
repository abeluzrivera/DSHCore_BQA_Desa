# Reporte Diario de Actividad de Contactos — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) 

**Goal:** Crear en DB_ODS el schema `reportes` con sus tablas, stored procedures y SQL Agent Jobs para generar diariamente el reporte de contactos y direcciones confirmados o agregados por asesor.

**Architecture:** Modelo de dos capas — tabla de detalle para drill-down y tabla de resumen pre-agregada para Power BI. Un job nocturno popula ambas capas a las 01:00 AM procesando el día anterior. Un job de baseline corre una sola vez para capturar el estado del día de migración como contexto.

**Tech Stack:** SQL Server 2019+, T-SQL, SQL Server Agent

## Global Constraints

- Base de datos destino: `DB_ODS`
- Schema nuevo: `reportes`
- Todos los scripts deben ser idempotentes (`CREATE OR ALTER`, `IF NOT EXISTS`)
- Tipos de contacto: 101=Celular, 102=Correo, 103=Telefono Convencional
- Tipos de dirección: 105=Domicilio, 106=Trabajo
- Filtro base obligatorio en todos los inserts: `Esta_Eliminado = 0`
- El día de migración se excluye de AGREGADOS (no de CONFIRMADOS)
- Nunca usar `SELECT *` en stored procedures de producción

---

## File Structure

| Archivo | Acción | Responsabilidad |
|---|---|---|
| `CI_DB/DB_ODS/Scripts/02_Schema_Tablas_Reportes.sql` | Crear | Schema + 4 tablas + índices |
| `CI_DB/DB_ODS/SP/SP_Baseline_Carga_Inicial.sql` | Crear | Lógica del job de baseline |
| `CI_DB/DB_ODS/SP/SP_Reporte_Actividad_Diaria.sql` | Crear | Lógica del job nocturno |
| `CI_DB/Jobs/Job_Baseline_Carga_Inicial.sql` | Crear | SQL Agent Job de baseline |
| `CI_DB/Jobs/Job_Reporte_Actividad_Diaria.sql` | Crear | SQL Agent Job nocturno |

---

## Task 1: Schema y Tablas DDL

**Files:**
- Create: `CI_DB/DB_ODS/Scripts/02_Schema_Tablas_Reportes.sql`

**Interfaces:**
- Produces: schema `reportes`, tablas `Tbl_Log_Jobs`, `Tbl_Baseline_Carga_Inicial`, `Tbl_Det_Actividad_Diaria`, `Tbl_Sum_Actividad_Diaria`

- [ ] **Step 1: Verificar que DB_ODS existe y que el schema `reportes` no existe aún**

```sql
USE DB_ODS;
GO
SELECT name FROM sys.schemas WHERE name = 'reportes';
-- Resultado esperado: 0 filas
```

- [ ] **Step 2: Crear el archivo `CI_DB/DB_ODS/Scripts/02_Schema_Tablas_Reportes.sql` con el siguiente contenido completo**

```sql
USE [DB_ODS]
GO

-- =============================================
-- Schema
-- =============================================
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'reportes')
BEGIN
    EXEC('CREATE SCHEMA [reportes] AUTHORIZATION [dbo]')
END
GO

-- =============================================
-- Tbl_Log_Jobs
-- =============================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[reportes].[Tbl_Log_Jobs]') AND type = 'U')
BEGIN
    CREATE TABLE [reportes].[Tbl_Log_Jobs] (
        [Id_Log]             int           IDENTITY(1,1) NOT NULL,
        [Nombre_Job]         nvarchar(100) NOT NULL,
        [Fecha_Reporte]      date          NULL,
        [Estado]             nvarchar(20)  NOT NULL,
        [Registros_Detalle]  int           NULL,
        [Registros_Resumen]  int           NULL,
        [Mensaje_Error]      nvarchar(max) NULL,
        [Fecha_Ejecucion]    datetime2(3)  NOT NULL,
        CONSTRAINT [PK_Tbl_Log_Jobs] PRIMARY KEY CLUSTERED ([Id_Log] ASC)
    )
END
GO

-- =============================================
-- Tbl_Baseline_Carga_Inicial
-- =============================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[reportes].[Tbl_Baseline_Carga_Inicial]') AND type = 'U')
BEGIN
    CREATE TABLE [reportes].[Tbl_Baseline_Carga_Inicial] (
        [Id_Baseline]                  int          IDENTITY(1,1) NOT NULL,
        [Fecha_Migracion]              date         NOT NULL,
        [Total_Contactos]              int          NOT NULL,
        [Total_Direcciones]            int          NOT NULL,
        [Contactos_Ya_Verificados]     int          NOT NULL,
        [Direcciones_Ya_Verificadas]   int          NOT NULL,
        [Fecha_Procesamiento]          datetime2(3) NOT NULL,
        CONSTRAINT [PK_Tbl_Baseline_Carga_Inicial] PRIMARY KEY CLUSTERED ([Id_Baseline] ASC)
    )
END
GO

-- =============================================
-- Tbl_Det_Actividad_Diaria
-- =============================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[reportes].[Tbl_Det_Actividad_Diaria]') AND type = 'U')
BEGIN
    CREATE TABLE [reportes].[Tbl_Det_Actividad_Diaria] (
        [Id_Detalle]            bigint        IDENTITY(1,1) NOT NULL,
        [Fecha_Reporte]         date          NOT NULL,
        [Tipo_Entidad]          nvarchar(15)  NOT NULL,
        [Id_Entidad]            int           NOT NULL,
        [Id_Cliente]            bigint        NOT NULL,
        [Tipo_Accion]           nvarchar(15)  NOT NULL,
        [Id_Tipo_Detalle]       int           NOT NULL,
        [Texto_Tipo]            nvarchar(50)  NOT NULL,
        [Valor]                 nvarchar(500) NULL,
        [Usuario_Responsable]   nvarchar(100) NOT NULL,
        [Source]                nvarchar(50)  NULL,
        [Fecha_Procesamiento]   datetime2(3)  NOT NULL,
        CONSTRAINT [PK_Tbl_Det_Actividad_Diaria] PRIMARY KEY CLUSTERED ([Id_Detalle] ASC)
    )
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Det_FechaReporte' AND object_id = OBJECT_ID('[reportes].[Tbl_Det_Actividad_Diaria]'))
    CREATE NONCLUSTERED INDEX [IX_Det_FechaReporte]
        ON [reportes].[Tbl_Det_Actividad_Diaria] ([Fecha_Reporte] ASC)
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Det_UsuarioFecha' AND object_id = OBJECT_ID('[reportes].[Tbl_Det_Actividad_Diaria]'))
    CREATE NONCLUSTERED INDEX [IX_Det_UsuarioFecha]
        ON [reportes].[Tbl_Det_Actividad_Diaria] ([Usuario_Responsable] ASC, [Fecha_Reporte] ASC)
GO

-- =============================================
-- Tbl_Sum_Actividad_Diaria
-- =============================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[reportes].[Tbl_Sum_Actividad_Diaria]') AND type = 'U')
BEGIN
    CREATE TABLE [reportes].[Tbl_Sum_Actividad_Diaria] (
        [Id_Resumen]            int          IDENTITY(1,1) NOT NULL,
        [Fecha_Reporte]         date         NOT NULL,
        [Tipo_Entidad]          nvarchar(15) NOT NULL,
        [Tipo_Accion]           nvarchar(15) NOT NULL,
        [Texto_Tipo]            nvarchar(50) NOT NULL,
        [Usuario_Responsable]   nvarchar(100) NOT NULL,
        [Total_Registros]       int          NOT NULL,
        [Fecha_Procesamiento]   datetime2(3) NOT NULL,
        CONSTRAINT [PK_Tbl_Sum_Actividad_Diaria] PRIMARY KEY CLUSTERED ([Id_Resumen] ASC)
    )
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Sum_FechaReporte' AND object_id = OBJECT_ID('[reportes].[Tbl_Sum_Actividad_Diaria]'))
    CREATE NONCLUSTERED INDEX [IX_Sum_FechaReporte]
        ON [reportes].[Tbl_Sum_Actividad_Diaria] ([Fecha_Reporte] ASC)
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Sum_Combinacion' AND object_id = OBJECT_ID('[reportes].[Tbl_Sum_Actividad_Diaria]'))
    CREATE UNIQUE NONCLUSTERED INDEX [UQ_Sum_Combinacion]
        ON [reportes].[Tbl_Sum_Actividad_Diaria] (
            [Fecha_Reporte], [Tipo_Entidad], [Tipo_Accion], [Texto_Tipo], [Usuario_Responsable]
        )
GO
```

- [ ] **Step 3: Ejecutar el script en DB_ODS y verificar**

```sql
USE DB_ODS;
GO
SELECT s.name AS schema_name, t.name AS tabla
FROM sys.tables t
JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE s.name = 'reportes'
ORDER BY t.name;
-- Resultado esperado: 4 filas
-- Tbl_Baseline_Carga_Inicial
-- Tbl_Det_Actividad_Diaria
-- Tbl_Log_Jobs
-- Tbl_Sum_Actividad_Diaria
```

---

## Task 2: SP_Baseline_Carga_Inicial

**Files:**
- Create: `CI_DB/DB_ODS/SP/SP_Baseline_Carga_Inicial.sql`

**Interfaces:**
- Consumes: `reportes.Tbl_Baseline_Carga_Inicial`, `reportes.Tbl_Log_Jobs`, `operativo.Tbl_Contacto_Cliente`, `operativo.Tbl_Direccion_Cliente`
- Produces: `reportes.SP_Baseline_Carga_Inicial` — sin parámetros, idempotente

- [ ] **Step 1: Verificar que las tablas del Task 1 existen antes de continuar**

```sql
USE DB_ODS;
SELECT OBJECT_ID('reportes.Tbl_Baseline_Carga_Inicial');
SELECT OBJECT_ID('reportes.Tbl_Log_Jobs');
-- Ambas deben retornar un valor distinto de NULL
```

- [ ] **Step 2: Crear el archivo `CI_DB/DB_ODS/SP/SP_Baseline_Carga_Inicial.sql` con el siguiente contenido**

```sql
USE [DB_ODS]
GO

CREATE OR ALTER PROCEDURE [reportes].[SP_Baseline_Carga_Inicial]
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @FechaMigracion          date;
    DECLARE @TotalContactos          int;
    DECLARE @TotalDirecciones        int;
    DECLARE @ContactosVerificados    int;
    DECLARE @DireccionesVerificadas  int;
    DECLARE @MensajeError            nvarchar(max);

    BEGIN TRY
        -- Idempotencia: salir si el baseline ya fue procesado
        IF EXISTS (SELECT 1 FROM reportes.Tbl_Baseline_Carga_Inicial)
        BEGIN
            RAISERROR('El baseline ya fue procesado anteriormente.', 16, 1);
            RETURN;
        END

        -- Detectar fecha de migración: el día con más registros en Tbl_Contacto_Cliente
        SELECT TOP 1
            @FechaMigracion = CAST(Fecha_Creacion AS date)
        FROM operativo.Tbl_Contacto_Cliente
        GROUP BY CAST(Fecha_Creacion AS date)
        ORDER BY COUNT(*) DESC;

        -- Totales del día de migración
        SELECT @TotalContactos = COUNT(*)
        FROM operativo.Tbl_Contacto_Cliente
        WHERE CAST(Fecha_Creacion AS date) = @FechaMigracion;

        SELECT @TotalDirecciones = COUNT(*)
        FROM operativo.Tbl_Direccion_Cliente
        WHERE CAST(Fecha_Creacion AS date) = @FechaMigracion;

        SELECT @ContactosVerificados = COUNT(*)
        FROM operativo.Tbl_Contacto_Cliente
        WHERE CAST(Fecha_Creacion AS date) = @FechaMigracion
          AND Fecha_Verificacion IS NOT NULL;

        SELECT @DireccionesVerificadas = COUNT(*)
        FROM operativo.Tbl_Direccion_Cliente
        WHERE CAST(Fecha_Creacion AS date) = @FechaMigracion
          AND Fecha_Verificacion IS NOT NULL;

        INSERT INTO reportes.Tbl_Baseline_Carga_Inicial (
            Fecha_Migracion,
            Total_Contactos,
            Total_Direcciones,
            Contactos_Ya_Verificados,
            Direcciones_Ya_Verificadas,
            Fecha_Procesamiento
        )
        VALUES (
            @FechaMigracion,
            @TotalContactos,
            @TotalDirecciones,
            @ContactosVerificados,
            @DireccionesVerificadas,
            SYSDATETIME()
        );

        INSERT INTO reportes.Tbl_Log_Jobs (
            Nombre_Job, Fecha_Reporte, Estado,
            Registros_Detalle, Registros_Resumen, Fecha_Ejecucion
        )
        VALUES (
            'SP_Baseline_Carga_Inicial', NULL, 'EXITOSO',
            1, 0, SYSDATETIME()
        );

    END TRY
    BEGIN CATCH
        SET @MensajeError = ERROR_MESSAGE();

        INSERT INTO reportes.Tbl_Log_Jobs (
            Nombre_Job, Fecha_Reporte, Estado,
            Mensaje_Error, Fecha_Ejecucion
        )
        VALUES (
            'SP_Baseline_Carga_Inicial', NULL, 'ERROR',
            @MensajeError, SYSDATETIME()
        );

        THROW;
    END CATCH
END
GO
```

- [ ] **Step 3: Ejecutar el script y verificar que el SP existe**

```sql
USE DB_ODS;
SELECT OBJECT_ID('reportes.SP_Baseline_Carga_Inicial');
-- Resultado esperado: valor distinto de NULL
```

- [ ] **Step 4: Ejecutar el SP y verificar el resultado**

```sql
USE DB_ODS;
EXEC reportes.SP_Baseline_Carga_Inicial;

SELECT * FROM reportes.Tbl_Baseline_Carga_Inicial;
-- Resultado esperado: 1 fila con la fecha de migración y los totales

SELECT * FROM reportes.Tbl_Log_Jobs;
-- Resultado esperado: 1 fila con Estado = 'EXITOSO'
```

- [ ] **Step 5: Verificar idempotencia — volver a ejecutar debe fallar con el mensaje controlado**

```sql
EXEC reportes.SP_Baseline_Carga_Inicial;
-- Resultado esperado: error controlado "El baseline ya fue procesado anteriormente."
-- NO debe insertar una segunda fila en Tbl_Baseline_Carga_Inicial

SELECT COUNT(*) FROM reportes.Tbl_Baseline_Carga_Inicial;
-- Resultado esperado: 1
```

---

## Task 3: SP_Reporte_Actividad_Diaria

**Files:**
- Create: `CI_DB/DB_ODS/SP/SP_Reporte_Actividad_Diaria.sql`

**Interfaces:**
- Consumes: `reportes.Tbl_Baseline_Carga_Inicial` (para obtener @FechaMigracion), `operativo.Tbl_Contacto_Cliente`, `operativo.Tbl_Direccion_Cliente`, `reportes.Tbl_Det_Actividad_Diaria`, `reportes.Tbl_Sum_Actividad_Diaria`, `reportes.Tbl_Log_Jobs`
- Produces: `reportes.SP_Reporte_Actividad_Diaria` — parámetro `@FechaReporte date = NULL` (NULL = ayer)

- [ ] **Step 1: Verificar precondición — baseline debe estar ejecutado**

```sql
USE DB_ODS;
SELECT Fecha_Migracion FROM reportes.Tbl_Baseline_Carga_Inicial;
-- Resultado esperado: 1 fila. Si está vacío, ejecutar Task 2 primero.
```

- [ ] **Step 2: Crear el archivo `CI_DB/DB_ODS/SP/SP_Reporte_Actividad_Diaria.sql`**

```sql
USE [DB_ODS]
GO

CREATE OR ALTER PROCEDURE [reportes].[SP_Reporte_Actividad_Diaria]
    @FechaReporte date = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @FechaMigracion   date;
    DECLARE @RowsDetalle      int = 0;
    DECLARE @RowsResumen      int = 0;
    DECLARE @MensajeError     nvarchar(max);

    BEGIN TRY
        -- Default: ayer
        IF @FechaReporte IS NULL
            SET @FechaReporte = CAST(DATEADD(day, -1, GETDATE()) AS date);

        -- Obtener fecha de migración
        SELECT @FechaMigracion = Fecha_Migracion
        FROM reportes.Tbl_Baseline_Carga_Inicial;

        -- No procesar si no existe baseline
        IF @FechaMigracion IS NULL
        BEGIN
            RAISERROR('No existe registro de baseline. Ejecutar SP_Baseline_Carga_Inicial primero.', 16, 1);
            RETURN;
        END

        -- No procesar el día de migración
        IF @FechaReporte = @FechaMigracion
        BEGIN
            INSERT INTO reportes.Tbl_Log_Jobs (
                Nombre_Job, Fecha_Reporte, Estado, Mensaje_Error, Fecha_Ejecucion
            )
            VALUES (
                'SP_Reporte_Actividad_Diaria', @FechaReporte, 'EXITOSO',
                'Fecha de migracion omitida intencionalmente', SYSDATETIME()
            );
            RETURN;
        END

        -- Idempotencia: limpiar datos previos del día
        DELETE FROM reportes.Tbl_Det_Actividad_Diaria WHERE Fecha_Reporte = @FechaReporte;
        DELETE FROM reportes.Tbl_Sum_Actividad_Diaria WHERE Fecha_Reporte = @FechaReporte;

        -- =============================================
        -- CONTACTOS CONFIRMADOS
        -- =============================================
        INSERT INTO reportes.Tbl_Det_Actividad_Diaria (
            Fecha_Reporte, Tipo_Entidad, Id_Entidad, Id_Cliente, Tipo_Accion,
            Id_Tipo_Detalle, Texto_Tipo, Valor, Usuario_Responsable, Source, Fecha_Procesamiento
        )
        SELECT
            @FechaReporte,
            'CONTACTO',
            c.Id_Contacto_Cliente,
            c.Id_Cliente,
            'CONFIRMADO',
            c.Id_Tipo_Contacto,
            CASE c.Id_Tipo_Contacto
                WHEN 101 THEN 'Celular'
                WHEN 102 THEN 'Correo'
                WHEN 103 THEN 'Telefono Convencional'
                ELSE 'Otro'
            END,
            c.Valor_Contacto,
            c.Usuario_Verificador,
            c.Source,
            SYSDATETIME()
        FROM operativo.Tbl_Contacto_Cliente c
        WHERE CAST(c.Fecha_Verificacion AS date) = @FechaReporte
          AND c.Esta_Eliminado = 0;

        -- =============================================
        -- CONTACTOS AGREGADOS
        -- =============================================
        INSERT INTO reportes.Tbl_Det_Actividad_Diaria (
            Fecha_Reporte, Tipo_Entidad, Id_Entidad, Id_Cliente, Tipo_Accion,
            Id_Tipo_Detalle, Texto_Tipo, Valor, Usuario_Responsable, Source, Fecha_Procesamiento
        )
        SELECT
            @FechaReporte,
            'CONTACTO',
            c.Id_Contacto_Cliente,
            c.Id_Cliente,
            'AGREGADO',
            c.Id_Tipo_Contacto,
            CASE c.Id_Tipo_Contacto
                WHEN 101 THEN 'Celular'
                WHEN 102 THEN 'Correo'
                WHEN 103 THEN 'Telefono Convencional'
                ELSE 'Otro'
            END,
            c.Valor_Contacto,
            c.Usuario_Creacion,
            c.Source,
            SYSDATETIME()
        FROM operativo.Tbl_Contacto_Cliente c
        WHERE CAST(c.Fecha_Creacion AS date) = @FechaReporte
          AND CAST(c.Fecha_Creacion AS date) <> @FechaMigracion
          AND c.Esta_Eliminado = 0;

        -- =============================================
        -- DIRECCIONES CONFIRMADAS
        -- =============================================
        INSERT INTO reportes.Tbl_Det_Actividad_Diaria (
            Fecha_Reporte, Tipo_Entidad, Id_Entidad, Id_Cliente, Tipo_Accion,
            Id_Tipo_Detalle, Texto_Tipo, Valor, Usuario_Responsable, Source, Fecha_Procesamiento
        )
        SELECT
            @FechaReporte,
            'DIRECCION',
            d.Id_Direccion_Cliente,
            d.Id_Cliente,
            'CONFIRMADO',
            ISNULL(d.Id_Tipo_Direccion, 0),
            CASE d.Id_Tipo_Direccion
                WHEN 105 THEN 'Domicilio'
                WHEN 106 THEN 'Trabajo'
                ELSE 'Otro'
            END,
            d.Direccion_Completa,
            d.Usuario_Verificador,
            d.Source_Direccion,
            SYSDATETIME()
        FROM operativo.Tbl_Direccion_Cliente d
        WHERE CAST(d.Fecha_Verificacion AS date) = @FechaReporte
          AND d.Esta_Eliminado = 0;

        -- =============================================
        -- DIRECCIONES AGREGADAS
        -- =============================================
        INSERT INTO reportes.Tbl_Det_Actividad_Diaria (
            Fecha_Reporte, Tipo_Entidad, Id_Entidad, Id_Cliente, Tipo_Accion,
            Id_Tipo_Detalle, Texto_Tipo, Valor, Usuario_Responsable, Source, Fecha_Procesamiento
        )
        SELECT
            @FechaReporte,
            'DIRECCION',
            d.Id_Direccion_Cliente,
            d.Id_Cliente,
            'AGREGADO',
            ISNULL(d.Id_Tipo_Direccion, 0),
            CASE d.Id_Tipo_Direccion
                WHEN 105 THEN 'Domicilio'
                WHEN 106 THEN 'Trabajo'
                ELSE 'Otro'
            END,
            d.Direccion_Completa,
            d.Usuario_Creacion,
            d.Source_Direccion,
            SYSDATETIME()
        FROM operativo.Tbl_Direccion_Cliente d
        WHERE CAST(d.Fecha_Creacion AS date) = @FechaReporte
          AND CAST(d.Fecha_Creacion AS date) <> @FechaMigracion
          AND d.Esta_Eliminado = 0;

        SELECT @RowsDetalle = COUNT(*)
        FROM reportes.Tbl_Det_Actividad_Diaria
        WHERE Fecha_Reporte = @FechaReporte;

        -- =============================================
        -- RESUMEN AGREGADO
        -- =============================================
        INSERT INTO reportes.Tbl_Sum_Actividad_Diaria (
            Fecha_Reporte, Tipo_Entidad, Tipo_Accion, Texto_Tipo,
            Usuario_Responsable, Total_Registros, Fecha_Procesamiento
        )
        SELECT
            Fecha_Reporte,
            Tipo_Entidad,
            Tipo_Accion,
            Texto_Tipo,
            Usuario_Responsable,
            COUNT(*),
            SYSDATETIME()
        FROM reportes.Tbl_Det_Actividad_Diaria
        WHERE Fecha_Reporte = @FechaReporte
        GROUP BY Fecha_Reporte, Tipo_Entidad, Tipo_Accion, Texto_Tipo, Usuario_Responsable;

        SELECT @RowsResumen = COUNT(*)
        FROM reportes.Tbl_Sum_Actividad_Diaria
        WHERE Fecha_Reporte = @FechaReporte;

        INSERT INTO reportes.Tbl_Log_Jobs (
            Nombre_Job, Fecha_Reporte, Estado,
            Registros_Detalle, Registros_Resumen, Fecha_Ejecucion
        )
        VALUES (
            'SP_Reporte_Actividad_Diaria', @FechaReporte, 'EXITOSO',
            @RowsDetalle, @RowsResumen, SYSDATETIME()
        );

    END TRY
    BEGIN CATCH
        SET @MensajeError = ERROR_MESSAGE();

        INSERT INTO reportes.Tbl_Log_Jobs (
            Nombre_Job, Fecha_Reporte, Estado,
            Mensaje_Error, Fecha_Ejecucion
        )
        VALUES (
            'SP_Reporte_Actividad_Diaria', @FechaReporte, 'ERROR',
            @MensajeError, SYSDATETIME()
        );

        THROW;
    END CATCH
END
GO
```

- [ ] **Step 3: Ejecutar el script y verificar que el SP existe**

```sql
USE DB_ODS;
SELECT OBJECT_ID('reportes.SP_Reporte_Actividad_Diaria');
-- Resultado esperado: valor distinto de NULL
```

- [ ] **Step 4: Ejecutar el SP con una fecha específica para prueba (usar una fecha con datos reales)**

```sql
USE DB_ODS;
-- Reemplazar con una fecha que tenga actividad real y que NO sea la fecha de migración
EXEC reportes.SP_Reporte_Actividad_Diaria @FechaReporte = '2026-07-05';

-- Verificar detalle
SELECT Tipo_Entidad, Tipo_Accion, Texto_Tipo, Usuario_Responsable, COUNT(*) AS Total
FROM reportes.Tbl_Det_Actividad_Diaria
WHERE Fecha_Reporte = '2026-07-05'
GROUP BY Tipo_Entidad, Tipo_Accion, Texto_Tipo, Usuario_Responsable
ORDER BY Tipo_Entidad, Tipo_Accion;

-- Verificar resumen
SELECT * FROM reportes.Tbl_Sum_Actividad_Diaria WHERE Fecha_Reporte = '2026-07-05';

-- Verificar log
SELECT * FROM reportes.Tbl_Log_Jobs ORDER BY Fecha_Ejecucion DESC;
```

- [ ] **Step 5: Verificar idempotencia — volver a ejecutar con la misma fecha**

```sql
EXEC reportes.SP_Reporte_Actividad_Diaria @FechaReporte = '2026-07-05';

-- Los conteos deben ser idénticos a los del Step 4, no duplicados
SELECT COUNT(*) FROM reportes.Tbl_Det_Actividad_Diaria WHERE Fecha_Reporte = '2026-07-05';
SELECT COUNT(*) FROM reportes.Tbl_Sum_Actividad_Diaria WHERE Fecha_Reporte = '2026-07-05';
```

- [ ] **Step 6: Verificar que el día de migración es omitido**

```sql
DECLARE @FechaMig date = (SELECT Fecha_Migracion FROM reportes.Tbl_Baseline_Carga_Inicial);
EXEC reportes.SP_Reporte_Actividad_Diaria @FechaReporte = @FechaMig;

SELECT Estado, Mensaje_Error
FROM reportes.Tbl_Log_Jobs
WHERE Fecha_Reporte = @FechaMig;
-- Resultado esperado: Estado = 'EXITOSO', Mensaje_Error = 'Fecha de migracion omitida intencionalmente'

SELECT COUNT(*) FROM reportes.Tbl_Det_Actividad_Diaria WHERE Fecha_Reporte = @FechaMig;
-- Resultado esperado: 0
```

---

## Task 4: SQL Agent Jobs

**Files:**
- Create: `CI_DB/Jobs/Job_Baseline_Carga_Inicial.sql`
- Create: `CI_DB/Jobs/Job_Reporte_Actividad_Diaria.sql`

**Interfaces:**
- Consumes: `reportes.SP_Baseline_Carga_Inicial`, `reportes.SP_Reporte_Actividad_Diaria`
- Produces: dos SQL Agent Jobs registrados en `msdb`

- [ ] **Step 1: Verificar que los SPs de Tasks 2 y 3 existen**

```sql
USE DB_ODS;
SELECT
    OBJECT_ID('reportes.SP_Baseline_Carga_Inicial') AS Baseline,
    OBJECT_ID('reportes.SP_Reporte_Actividad_Diaria') AS Diario;
-- Ambos deben retornar valor distinto de NULL
```

- [ ] **Step 2: Crear `CI_DB/Jobs/Job_Baseline_Carga_Inicial.sql`**

```sql
USE [msdb]
GO

-- Eliminar si existe (idempotencia)
IF EXISTS (SELECT 1 FROM msdb.dbo.sysjobs WHERE name = N'JOB_Baseline_Carga_Inicial')
    EXEC msdb.dbo.sp_delete_job @job_name = N'JOB_Baseline_Carga_Inicial', @delete_unused_schedule = 1;
GO

EXEC msdb.dbo.sp_add_job
    @job_name        = N'JOB_Baseline_Carga_Inicial',
    @description     = N'Ejecucion unica para capturar la linea base del dia de migracion en DB_ODS',
    @category_name   = N'[Uncategorized (Local)]',
    @owner_login_name = N'sa',
    @enabled         = 1;
GO

EXEC msdb.dbo.sp_add_jobstep
    @job_name       = N'JOB_Baseline_Carga_Inicial',
    @step_name      = N'Ejecutar SP_Baseline_Carga_Inicial',
    @subsystem      = N'TSQL',
    @command        = N'EXEC DB_ODS.reportes.SP_Baseline_Carga_Inicial;',
    @database_name  = N'DB_ODS',
    @on_success_action = 1,  -- Quit with success
    @on_fail_action    = 2;  -- Quit with failure
GO

EXEC msdb.dbo.sp_add_jobserver
    @job_name   = N'JOB_Baseline_Carga_Inicial',
    @server_name = N'(LOCAL)';
GO
```

- [ ] **Step 3: Crear `CI_DB/Jobs/Job_Reporte_Actividad_Diaria.sql`**

```sql
USE [msdb]
GO

-- Eliminar si existe (idempotencia)
IF EXISTS (SELECT 1 FROM msdb.dbo.sysjobs WHERE name = N'JOB_Reporte_Actividad_Diaria')
    EXEC msdb.dbo.sp_delete_job @job_name = N'JOB_Reporte_Actividad_Diaria', @delete_unused_schedule = 1;
GO

EXEC msdb.dbo.sp_add_job
    @job_name        = N'JOB_Reporte_Actividad_Diaria',
    @description     = N'Genera diariamente el reporte de contactos y direcciones confirmados o agregados por asesor. Corre a la 01:00 AM procesando el dia anterior.',
    @category_name   = N'[Uncategorized (Local)]',
    @owner_login_name = N'sa',
    @enabled         = 1;
GO

EXEC msdb.dbo.sp_add_jobstep
    @job_name       = N'JOB_Reporte_Actividad_Diaria',
    @step_name      = N'Ejecutar SP_Reporte_Actividad_Diaria',
    @subsystem      = N'TSQL',
    @command        = N'EXEC DB_ODS.reportes.SP_Reporte_Actividad_Diaria;',
    @database_name  = N'DB_ODS',
    @on_success_action = 1,
    @on_fail_action    = 2;
GO

-- Schedule: diario a la 01:00 AM
EXEC msdb.dbo.sp_add_schedule
    @schedule_name          = N'SCH_Diario_01AM',
    @freq_type              = 4,        -- Daily
    @freq_interval          = 1,        -- Every 1 day
    @active_start_time      = 10000,    -- 01:00:00 AM
    @active_start_date      = 20260706;
GO

EXEC msdb.dbo.sp_attach_schedule
    @job_name      = N'JOB_Reporte_Actividad_Diaria',
    @schedule_name = N'SCH_Diario_01AM';
GO

EXEC msdb.dbo.sp_add_jobserver
    @job_name    = N'JOB_Reporte_Actividad_Diaria',
    @server_name = N'(LOCAL)';
GO
```

- [ ] **Step 4: Ejecutar ambos scripts y verificar que los jobs fueron creados**

```sql
USE msdb;
SELECT name, enabled, description
FROM msdb.dbo.sysjobs
WHERE name IN ('JOB_Baseline_Carga_Inicial', 'JOB_Reporte_Actividad_Diaria');
-- Resultado esperado: 2 filas con enabled = 1
```

- [ ] **Step 5: Verificar el schedule del job diario**

```sql
SELECT j.name AS job_name, s.name AS schedule_name,
       s.freq_type, s.active_start_time
FROM msdb.dbo.sysjobs j
JOIN msdb.dbo.sysjobschedules js ON js.job_id = j.job_id
JOIN msdb.dbo.sysschedules s ON s.schedule_id = js.schedule_id
WHERE j.name = 'JOB_Reporte_Actividad_Diaria';
-- Resultado esperado: active_start_time = 10000 (01:00 AM)
```

- [ ] **Step 6: Ejecutar el job de baseline desde SQL Server Agent y confirmar resultado**

```sql
-- Ejecutar manualmente desde SSMS: clic derecho en JOB_Baseline_Carga_Inicial > Start Job
-- O via T-SQL:
EXEC msdb.dbo.sp_start_job @job_name = N'JOB_Baseline_Carga_Inicial';

-- Esperar ~5 segundos y verificar
SELECT * FROM DB_ODS.reportes.Tbl_Baseline_Carga_Inicial;
SELECT * FROM DB_ODS.reportes.Tbl_Log_Jobs ORDER BY Fecha_Ejecucion DESC;
-- Resultado esperado: 1 fila en baseline, log con Estado = 'EXITOSO'
```

- [ ] **Step 7: Ejecutar el job diario manualmente con fecha de prueba para validación final**

```sql
-- Probar ejecucion directa del SP con fecha real antes de dejar el job en produccion
USE DB_ODS;
EXEC reportes.SP_Reporte_Actividad_Diaria @FechaReporte = '2026-07-05';

SELECT
    Fecha_Reporte,
    Tipo_Entidad,
    Tipo_Accion,
    Texto_Tipo,
    SUM(Total_Registros) AS Total
FROM reportes.Tbl_Sum_Actividad_Diaria
WHERE Fecha_Reporte = '2026-07-05'
GROUP BY Fecha_Reporte, Tipo_Entidad, Tipo_Accion, Texto_Tipo
ORDER BY Tipo_Entidad, Tipo_Accion, Texto_Tipo;
-- Resultado esperado: filas con totales por tipo de entidad, accion y tipo de contacto/direccion
```
