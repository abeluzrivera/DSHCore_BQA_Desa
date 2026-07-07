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
