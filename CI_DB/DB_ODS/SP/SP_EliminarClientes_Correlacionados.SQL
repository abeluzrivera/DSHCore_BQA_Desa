USE [DB_ODS_STG]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [operativo].[SP_EliminarClientes_Correlacionados]
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Iniciamos una transacción. Si algo falla, revertimos todo.
        BEGIN TRANSACTION;

        -- 1. Identificamos los clientes exactos que vamos a eliminar.
        -- Usamos una tabla temporal para que el motor evalúe el cruce una sola vez.
        -- Cruzamos la tabla de ODS_STG con la de CONTACTABILIDAD usando la identificación.
        SELECT C.Id_Cliente
        INTO #ClientesAEliminar
        FROM [operativo].[Tbl_Maest_Cliente] C
        INNER JOIN [DB_CONTACTABILIDAD].[dbo].[Empleados_Raw] E
            ON C.Identificacion_Cliente = E.identificacion COLLATE DATABASE_DEFAULT;

        -- 2 Eliminamos las dependencias en las TABLAS HIJAS
        DELETE FROM [operativo].[Tbl_Direccion_Cliente]
        WHERE Id_Cliente IN (SELECT Id_Cliente FROM #ClientesAEliminar);

        DELETE FROM [operativo].[Tbl_Contacto_Cliente]
        WHERE Id_Cliente IN (SELECT Id_Cliente FROM #ClientesAEliminar);

        DELETE FROM [operativo].[Tbl_Financiero_cliente]
        WHERE Id_Cliente IN (SELECT Id_Cliente FROM #ClientesAEliminar);
         DELETE FROM [operativo].[Tbl_Oficializacion_Core]
        WHERE Id_Cliente IN (SELECT Id_Cliente FROM #ClientesAEliminar);

        -- 3. Finalmente, eliminamos los registros en la TABLA MAESTRA
        DELETE FROM [operativo].[Tbl_Maest_Cliente]
        WHERE Id_Cliente IN (SELECT Id_Cliente FROM #ClientesAEliminar);

        -- Limpiamos la tabla temporal de la memoria
        DROP TABLE #ClientesAEliminar;

        -- Si todo salió bien, confirmamos los cambios
        COMMIT TRANSACTION;

    END TRY
    BEGIN CATCH
        -- Si ocurre algún error en cualquier paso, deshacemos todos los cambios
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        -- Capturamos y mostramos el error
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR (@ErrorMessage, 16, 1);
    END CATCH
END
GO