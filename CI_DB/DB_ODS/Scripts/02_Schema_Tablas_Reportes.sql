USE [DB_ODS]
GO

-- =============================================
-- Schema reportes
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
        [Id_Log]            int           IDENTITY(1,1) NOT NULL,
        [Nombre_Job]        nvarchar(100) NOT NULL,
        [Fecha_Reporte]     date          NULL,
        [Estado]            nvarchar(20)  NOT NULL,
        [Registros_Detalle] int           NULL,
        [Registros_Resumen] int           NULL,
        [Mensaje_Error]     nvarchar(max) NULL,
        [Fecha_Ejecucion]   datetime2(3)  NOT NULL,
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
        [Id_Baseline]                int          IDENTITY(1,1) NOT NULL,
        [Fecha_Migracion]            date         NOT NULL,
        [Total_Contactos]            int          NOT NULL,
        [Total_Direcciones]          int          NOT NULL,
        [Contactos_Ya_Verificados]   int          NOT NULL,
        [Direcciones_Ya_Verificadas] int          NOT NULL,
        [Fecha_Procesamiento]        datetime2(3) NOT NULL,
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
        [Id_Detalle]          bigint        IDENTITY(1,1) NOT NULL,
        [Fecha_Reporte]       date          NOT NULL,
        [Tipo_Entidad]        nvarchar(15)  NOT NULL,
        [Id_Entidad]          int           NOT NULL,
        [Id_Cliente]          bigint        NOT NULL,
        [Tipo_Accion]         nvarchar(15)  NOT NULL,
        [Id_Tipo_Detalle]     int           NOT NULL,
        [Texto_Tipo]          nvarchar(50)  NOT NULL,
        [Valor]               nvarchar(500) NULL,
        [Usuario_Responsable] nvarchar(100) NOT NULL,
        [Source]              nvarchar(50)  NULL,
        [Fecha_Procesamiento] datetime2(3)  NOT NULL,
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
        [Id_Resumen]          int          IDENTITY(1,1) NOT NULL,
        [Fecha_Reporte]       date         NOT NULL,
        [Tipo_Entidad]        nvarchar(15) NOT NULL,
        [Tipo_Accion]         nvarchar(15) NOT NULL,
        [Texto_Tipo]          nvarchar(50) NOT NULL,
        [Usuario_Responsable] nvarchar(100) NOT NULL,
        [Total_Registros]     int          NOT NULL,
        [Fecha_Procesamiento] datetime2(3) NOT NULL,
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

-- =============================================
-- Verificacion post-ejecucion
-- =============================================
SELECT s.name AS schema_name, t.name AS tabla
FROM sys.tables t
JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE s.name = 'reportes'
ORDER BY t.name;
-- Resultado esperado: 4 filas
