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
