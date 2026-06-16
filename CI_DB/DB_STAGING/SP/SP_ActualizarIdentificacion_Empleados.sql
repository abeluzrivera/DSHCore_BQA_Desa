USE [DB_CONTACTABILIDAD]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Creamos el Procedimiento Almacenado
CREATE OR ALTER PROCEDURE [dbo].[SP_ActualizarIdentificacion_Empleados]
AS
BEGIN
    -- SET NOCOUNT ON evita que se devuelva el mensaje de "X filas afectadas",
    -- lo cual mejora el rendimiento de ejecución.
    SET NOCOUNT ON;

    BEGIN TRY
        -- Actualizamos el campo 'identificacion'
        UPDATE [dbo].[Empleados_Raw]
        SET identificacion = CASE 
                                -- Si la cédula empieza con '0', usamos STUFF para eliminar el primer carácter
                                WHEN cedula LIKE '0%' THEN STUFF(cedula, 1, 1, '')
                                -- Si no empieza con '0', simplemente copiamos la cédula tal cual
                                ELSE cedula
                             END;
    END TRY
    BEGIN CATCH
        -- Manejo de errores básico
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR (@ErrorMessage, 16, 1);
    END CATCH
END
GO