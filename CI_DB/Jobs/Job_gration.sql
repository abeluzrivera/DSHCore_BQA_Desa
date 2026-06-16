SET NOCOUNT ON;
SET XACT_ABORT ON; -- Rollback automático en errores

-- =================================================================
-- SCRIPT DE MIGRACIÓN MASIVA Y ENRIQUECIMIENTO - OPTIMIZADO Y CORREGIDO
-- Versión: 2.0
-- =================================================================

DECLARE @BatchSize INT = 50000;
DECLARE @RowsAffected INT = 1;
DECLARE @TotalInserted INT = 0;
DECLARE @TotalToLoad INT = 0;
DECLARE @TotalUpdated INT = 0;
DECLARE @UserJob NVARCHAR(50) = 'SQLJOBMigrate';
DECLARE @Source NVARCHAR(15) = 'EQUIFAX';
DECLARE @Pais NVARCHAR(20) = 'ECUADOR';
DECLARE @FechaEjecucion DATETIME2 = SYSDATETIME(); -- ✅ Calcular una sola vez

PRINT '=======================================================';
PRINT 'PASO 0: Cargando IDs de Catálogos (Master Data Management)';
PRINT '=======================================================';

DECLARE @IdEstadoPendiente INT;
DECLARE @IdTipoContactoCel INT;
DECLARE @IdTipoContactoTel INT;
DECLARE @IdTipoContactoEmail INT;
DECLARE @IdTipoDireccion INT;
DECLARE @IdTipoIdentificacionCedula INT = 201;
DECLARE @IdEstadoPendienteLOPDP INT = 6;

-- ✅ Consolidar en una sola consulta
SELECT 
    @IdEstadoPendiente = MAX(CASE WHEN g.Nombre_Grupo = 'ESTADO_CONTACTABILIDAD' AND i.Codigo_Valor = 'PEND' THEN i.Id_Item_Catalogo END),
    @IdTipoContactoCel = MAX(CASE WHEN g.Nombre_Grupo = 'TIPO_CONTACTO' AND i.Codigo_Valor = 'CEL' THEN i.Id_Item_Catalogo END),
    @IdTipoContactoTel = MAX(CASE WHEN g.Nombre_Grupo = 'TIPO_CONTACTO' AND i.Codigo_Valor = 'TEL_C' THEN i.Id_Item_Catalogo END),
    @IdTipoContactoEmail = MAX(CASE WHEN g.Nombre_Grupo = 'TIPO_CONTACTO' AND i.Codigo_Valor = 'EMAIL' THEN i.Id_Item_Catalogo END),
    @IdTipoDireccion = MAX(CASE WHEN g.Nombre_Grupo = 'TIPO_CONTACTO' AND i.Codigo_Valor = 'DIR_D' THEN i.Id_Item_Catalogo END)
FROM parametro.Tbl_Cat_Item i
INNER JOIN parametro.Tbl_Cat_Grupo g ON i.Id_Grupo_Catalogo = g.Id_Grupo_Catalogo
WHERE (g.Nombre_Grupo = 'ESTADO_CONTACTABILIDAD' AND i.Codigo_Valor = 'PEND')
   OR (g.Nombre_Grupo = 'TIPO_CONTACTO' AND i.Codigo_Valor IN ('CEL', 'TEL_C', 'EMAIL', 'DIR_D'));

PRINT CONCAT('IDs Catálogo: Estado=', @IdEstadoPendiente, ', Cel=', @IdTipoContactoCel, ', Tel=', @IdTipoContactoTel, ', Email=', @IdTipoContactoEmail, ', Dir=', @IdTipoDireccion);

-- Validación de Gobernanza
IF (@IdEstadoPendiente IS NULL OR @IdTipoContactoCel IS NULL OR @IdTipoIdentificacionCedula IS NULL)
BEGIN
    RAISERROR ('Fallo de Integridad: No se encontraron los catálogos base. Ejecute el Seeder de EF Core primero.', 16, 1);
    RETURN;
END

PRINT '=======================================================';
PRINT 'PASO 1: Consolidando el universo de datos en TempDB...';
PRINT '=======================================================';

-- Limpiar tablas temporales si existen
IF OBJECT_ID('tempdb..#MigracionRaw') IS NOT NULL DROP TABLE #MigracionRaw;
IF OBJECT_ID('tempdb..#MigracionData') IS NOT NULL DROP TABLE #MigracionData;
IF OBJECT_ID('tempdb..#ClientesExistentes') IS NOT NULL DROP TABLE #ClientesExistentes;
IF OBJECT_ID('tempdb..#InsertedClients') IS NOT NULL DROP TABLE #InsertedClients;
IF OBJECT_ID('tempdb..#ReporteNoInsertados') IS NOT NULL DROP TABLE #ReporteNoInsertados;
IF OBJECT_ID('tempdb..#ContactosExistentes') IS NOT NULL DROP TABLE #ContactosExistentes;
IF OBJECT_ID('tempdb..#DireccionesExistentes') IS NOT NULL DROP TABLE #DireccionesExistentes;

CREATE TABLE #ReporteNoInsertados (
    Identificacion VARCHAR(50),
    Motivo VARCHAR(100)
);

-- 1. UNIVERSO DE DATOS EN MEMORIA CON LIMPIEZA DE DATOS
WITH Contactabilidad AS (
    SELECT 
        a.Identificacion, 
        a.NombreCompleto, 
        LTRIM(RTRIM(a.Telefono1)) AS Telefono1, 
        LTRIM(RTRIM(a.Telefono2)) AS Telefono2, 
        LTRIM(RTRIM(a.Telefono3)) AS Telefono3, 
        LTRIM(RTRIM(a.Telefono4)) AS Telefono4, 
        LTRIM(RTRIM(a.Telefono5)) AS Telefono5,
        LTRIM(RTRIM(a.Telefono6)) AS Telefono6,  
        LTRIM(RTRIM(a.Email1)) AS Email1, 
        LTRIM(RTRIM(a.Email2)) AS Email2, 
        LTRIM(RTRIM(a.Email3)) AS Email3, 
        LTRIM(RTRIM(a.Email4)) AS Email4, 
        LTRIM(RTRIM(a.Email5)) AS Email5, 
        LTRIM(RTRIM(a.Email6)) AS Email6
    FROM DB_CONTACTABILIDAD.dbo.Tbl_Contactabilidad a
),
Direcciones AS (
    SELECT 
        a.identificacion, 
        a.nombre_sujeto AS Nombre_Completo, 
        LTRIM(RTRIM(a.direccion1)) AS direccion1,
        LTRIM(RTRIM(a.direccion2)) AS direccion2,
        LTRIM(RTRIM(a.direccion3)) AS direccion3,
        LTRIM(RTRIM(a.direccion4)) AS direccion4,
        LTRIM(RTRIM(a.direccion5)) AS direccion5,
        LTRIM(RTRIM(a.telefono_fijo1)) AS telefono_fijo1,
        LTRIM(RTRIM(a.telefono_fijo2)) AS telefono_fijo2,
        LTRIM(RTRIM(a.telefono_fijo3)) AS telefono_fijo3,
        LTRIM(RTRIM(a.telefono_fijo4)) AS telefono_fijo4,
        LTRIM(RTRIM(a.telefono_fijo5)) AS telefono_fijo5,
        LTRIM(RTRIM(a.telefono_movil1)) AS telefono_movil1,
        LTRIM(RTRIM(a.telefono_movil2)) AS telefono_movil2,
        LTRIM(RTRIM(a.telefono_movil3)) AS telefono_movil3,
        LTRIM(RTRIM(a.telefono_movil4)) AS telefono_movil4,
        LTRIM(RTRIM(a.telefono_movil5)) AS telefono_movil5,
        a.ciudad1, a.ciudad2, a.ciudad3, a.ciudad4, a.ciudad5
    FROM DB_CONTACTABILIDAD.dbo.tbl_maestrodirecciones a
    WHERE a.direccion1 <> ''
),
GeoDireccion AS (
    SELECT 
        a.identificacion, 
        LTRIM(RTRIM(a.direccion)) AS direccion, 
        a.canton, 
        a.provincia, 
        a.parroquia, 
        a.longitud, 
        a.latitud
    FROM DB_CONTACTABILIDAD.dbo.ScoreMiDireccion a
)
SELECT
    COALESCE(c.Identificacion, d.identificacion, g.identificacion) AS Identificacion,
    COALESCE(c.NombreCompleto, d.Nombre_Completo, '') AS NombreCompleto,
    c.Telefono1, c.Telefono2, c.Telefono3, c.Telefono4, c.Telefono5, c.Telefono6,
    c.Email1, c.Email2, c.Email3, c.Email4, c.Email5, c.Email6,
    d.direccion1, d.direccion2, d.direccion3, d.direccion4, d.direccion5,
    d.telefono_fijo1, d.telefono_fijo2, d.telefono_fijo3, d.telefono_fijo4, d.telefono_fijo5,
    d.telefono_movil1, d.telefono_movil2, d.telefono_movil3, d.telefono_movil4, d.telefono_movil5,
    d.ciudad1, d.ciudad2, d.ciudad3, d.ciudad4, d.ciudad5,
    g.direccion, g.canton, g.provincia, g.parroquia, g.longitud, g.latitud
INTO #MigracionRaw
FROM Contactabilidad c
FULL OUTER JOIN Direcciones d ON c.identificacion = d.identificacion
FULL OUTER JOIN GeoDireccion g ON COALESCE(c.identificacion, d.identificacion) = g.identificacion
WHERE COALESCE(c.Identificacion, d.identificacion, g.identificacion) IS NOT NULL;

PRINT 'PASO 2: Depurando duplicados...';

INSERT INTO #ReporteNoInsertados (Identificacion, Motivo)
SELECT Identificacion, 'Duplicado en origen (Se cargó la primera ocurrencia)'
FROM #MigracionRaw 
GROUP BY Identificacion 
HAVING COUNT(*) > 1;

-- ✅ Listar columnas explícitamente
WITH CTE_Clean AS (
    SELECT 
        Identificacion, NombreCompleto,
        Telefono1, Telefono2, Telefono3, Telefono4, Telefono5, Telefono6,   Telefono6
        Email1, Email2, Email3, Email4, Email5, Email6,
        direccion1, direccion2, direccion3, direccion4, direccion5,
        telefono_fijo1, telefono_fijo2, telefono_fijo3, telefono_fijo4, telefono_fijo5,
        telefono_movil1, telefono_movil2, telefono_movil3, telefono_movil4, telefono_movil5,
        ciudad1, ciudad2, ciudad3, ciudad4, ciudad5,
        direccion, canton, provincia, parroquia, longitud, latitud,
        ROW_NUMBER() OVER(PARTITION BY Identificacion ORDER BY Identificacion) as rn
    FROM #MigracionRaw
)
SELECT 
    Identificacion, NombreCompleto,
    Telefono1, Telefono2, Telefono3, Telefono4, Telefono5, Telefono6,   Telefono6
    Email1, Email2, Email3, Email4, Email5, Email6,
    direccion1, direccion2, direccion3, direccion4, direccion5,
    telefono_fijo1, telefono_fijo2, telefono_fijo3, telefono_fijo4, telefono_fijo5,
    telefono_movil1, telefono_movil2, telefono_movil3, telefono_movil4, telefono_movil5,
    ciudad1, ciudad2, ciudad3, ciudad4, ciudad5,
    direccion, canton, provincia, parroquia, longitud, latitud
INTO #MigracionData 
FROM CTE_Clean 
WHERE rn = 1;

PRINT 'PASO 3: Enriqueciendo Clientes Existentes...';

SELECT 
    m.Identificacion, m.NombreCompleto,
    m.Telefono1, m.Telefono2, m.Telefono3, m.Telefono4, m.Telefono5, m.Telefono6, 
    m.Email1, m.Email2, m.Email3, m.Email4, m.Email5, m.Email6,
    m.direccion1, m.direccion2, m.direccion3, m.direccion4, m.direccion5,
    m.telefono_fijo1, m.telefono_fijo2, m.telefono_fijo3, m.telefono_fijo4, m.telefono_fijo5,
    m.telefono_movil1, m.telefono_movil2, m.telefono_movil3, m.telefono_movil4, m.telefono_movil5,
    m.ciudad1, m.ciudad2, m.ciudad3, m.ciudad4, m.ciudad5,
    m.direccion, m.canton, m.provincia, m.parroquia, m.longitud, m.latitud,
    dest.Id_Cliente
INTO #ClientesExistentes
FROM #MigracionData m
INNER JOIN operativo.Tbl_Maest_Cliente dest 
    ON m.Identificacion = dest.Identificacion_Cliente
WHERE dest.Esta_Eliminado = 0;

SELECT @TotalUpdated = @@ROWCOUNT;

IF @TotalUpdated > 0
BEGIN
    BEGIN TRY
        BEGIN TRAN;
        
        -- ✅ Crear tabla temporal de contactos existentes
        SELECT DISTINCT 
            Id_Cliente, 
            LTRIM(RTRIM(Valor_Contacto)) AS Valor_Contacto,
            Id_Tipo_Contacto
        INTO #ContactosExistentes
        FROM operativo.Tbl_Contacto_Cliente
        WHERE Id_Cliente IN (SELECT Id_Cliente FROM #ClientesExistentes)
          AND Esta_Eliminado = 0;
        
        CREATE CLUSTERED INDEX IX_ContactosExistentes ON #ContactosExistentes(Id_Cliente, Valor_Contacto, Id_Tipo_Contacto);
        
        -- ✅ Insertar contactos nuevos (NOMBRES EN MINÚSCULAS Y TELEFONO6 INCLUIDO)
        INSERT INTO operativo.Tbl_Contacto_Cliente (
            Id_Cliente, Id_Tipo_Contacto, Valor_Contacto, Id_Estado_Verificacion, 
            Fecha_Creacion, Usuario_Creacion, Esta_Eliminado, Source, Id_Estado_LOPDP
        )
        SELECT 
            ce.Id_Cliente, 
            contacto.Id_Tipo_Contacto, 
            contacto.Valor, 
            @IdEstadoPendiente, 
            @FechaEjecucion, 
            @UserJob, 
            0, 
            @Source, 
            @IdEstadoPendienteLOPDP
        FROM #ClientesExistentes ce
        CROSS APPLY (
            VALUES 
                -- ✅ Teléfonos de Contactabilidad (6 columnas, nombres correctos)
                (@IdTipoContactoCel, ce.Telefono1), 
                (@IdTipoContactoCel, ce.Telefono2), 
                (@IdTipoContactoCel, ce.Telefono3), 
                (@IdTipoContactoCel, ce.Telefono4), 
                (@IdTipoContactoCel, ce.Telefono5),
                (@IdTipoContactoCel, ce.Telefono6),  
                -- ✅ Teléfonos móviles (minúsculas)
                (@IdTipoContactoCel, ce.telefono_movil1), 
                (@IdTipoContactoCel, ce.telefono_movil2), 
                (@IdTipoContactoCel, ce.telefono_movil3), 
                (@IdTipoContactoCel, ce.telefono_movil4), 
                (@IdTipoContactoCel, ce.telefono_movil5),
                -- ✅ Teléfonos fijos (minúsculas)
                (@IdTipoContactoTel, ce.telefono_fijo1), 
                (@IdTipoContactoTel, ce.telefono_fijo2), 
                (@IdTipoContactoTel, ce.telefono_fijo3),
                (@IdTipoContactoTel, ce.telefono_fijo4), 
                (@IdTipoContactoTel, ce.telefono_fijo5),
                -- ✅ Emails
                (@IdTipoContactoEmail, ce.Email1), 
                (@IdTipoContactoEmail, ce.Email2), 
                (@IdTipoContactoEmail, ce.Email3), 
                (@IdTipoContactoEmail, ce.Email4), 
                (@IdTipoContactoEmail, ce.Email5), 
                (@IdTipoContactoEmail, ce.Email6)
        ) AS contacto(Id_Tipo_Contacto, Valor)
        LEFT JOIN #ContactosExistentes cex 
            ON cex.Id_Cliente = ce.Id_Cliente 
            AND cex.Valor_Contacto = LTRIM(RTRIM(contacto.Valor))
            AND cex.Id_Tipo_Contacto = contacto.Id_Tipo_Contacto
        WHERE contacto.Valor IS NOT NULL 
          AND LTRIM(RTRIM(contacto.Valor)) <> ''
          AND cex.Id_Cliente IS NULL;

        -- ✅ Crear tabla temporal de direcciones existentes
        SELECT DISTINCT 
            Id_Cliente, 
            LTRIM(RTRIM(Direccion_Completa)) AS Direccion_Completa
        INTO #DireccionesExistentes
        FROM operativo.Tbl_Direccion_Cliente
        WHERE Id_Cliente IN (SELECT Id_Cliente FROM #ClientesExistentes)
          AND Esta_Eliminado = 0;
        
        CREATE CLUSTERED INDEX IX_DireccionesExistentes ON #DireccionesExistentes(Id_Cliente, Direccion_Completa);
        
        -- ✅ Insertar direcciones nuevas (minúsculas)
        INSERT INTO operativo.Tbl_Direccion_Cliente (
            Id_Cliente, Id_Tipo_Direccion, Direccion_Completa, Ciudad, Provincia, Pais, 
            Parroquia, Latitud, Longitud, Es_Principal, Estado_Verificacion, 
            Fecha_Creacion, Usuario_Creacion, Source_Direccion, Esta_Eliminado
        )
        SELECT 
            ce.Id_Cliente, 
            @IdTipoDireccion, 
            dir.Direccion, 
            MAX(dir.Ciudad), 
            MAX(dir.Provincia), 
            @Pais, 
            MAX(dir.Parroquia), 
            MAX(dir.Latitud), 
            MAX(dir.Longitud), 
            0, 
            @IdEstadoPendiente, 
            @FechaEjecucion, 
            @UserJob, 
            @Source, 
            0
        FROM #ClientesExistentes ce
        CROSS APPLY (
            VALUES 
                (ce.direccion1, ce.ciudad1, NULL, NULL, NULL, NULL),
                (ce.direccion2, ce.ciudad2, NULL, NULL, NULL, NULL),
                (ce.direccion3, ce.ciudad3, NULL, NULL, NULL, NULL),
                (ce.direccion4, ce.ciudad4, NULL, NULL, NULL, NULL),
                (ce.direccion5, ce.ciudad5, NULL, NULL, NULL, NULL),
                (ce.direccion, ce.canton, ce.provincia, ce.parroquia, ce.latitud, ce.longitud)
        ) AS dir(Direccion, Ciudad, Provincia, Parroquia, Latitud, Longitud)
        LEFT JOIN #DireccionesExistentes dex 
            ON dex.Id_Cliente = ce.Id_Cliente 
            AND dex.Direccion_Completa = LTRIM(RTRIM(dir.Direccion))
        WHERE dir.Direccion IS NOT NULL 
          AND LTRIM(RTRIM(dir.Direccion)) <> ''
          AND dex.Id_Cliente IS NULL
        GROUP BY ce.Id_Cliente, dir.Direccion;

        COMMIT TRAN;
        
        INSERT INTO #ReporteNoInsertados (Identificacion, Motivo)
        SELECT Identificacion, 'Ya existía. Se evaluó y enriqueció sus contactos/direcciones'
        FROM #ClientesExistentes;

        DELETE m 
        FROM #MigracionData m 
        INNER JOIN #ClientesExistentes ce ON m.Identificacion = ce.Identificacion;
        
        PRINT 'Se enriquecieron ' + CAST(@TotalUpdated AS VARCHAR) + ' clientes existentes.';
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorLine INT = ERROR_LINE();
        RAISERROR ('Error en enriquecimiento de clientes (Línea %d): %s', 16, 1, @ErrorLine, @ErrorMessage);
        RETURN;
    END CATCH
END

SELECT @TotalToLoad = COUNT(*) FROM #MigracionData;

CREATE CLUSTERED INDEX IX_MigracionData_Identificacion ON #MigracionData(Identificacion);

-- ✅ Crear tabla temporal con índice
CREATE TABLE #InsertedClients (
    Id_Cliente BIGINT, 
    Identificacion_Cliente VARCHAR(50)
);

CREATE CLUSTERED INDEX IX_InsertedClients ON #InsertedClients(Identificacion_Cliente);

PRINT '=======================================================';
PRINT 'Total de clientes 100% NUEVOS a insertar: ' + CAST(@TotalToLoad AS VARCHAR);
PRINT 'Iniciando migración en lotes de ' + CAST(@BatchSize AS VARCHAR) + '...';
PRINT '=======================================================';

-- 4. BUCLE PRINCIPAL (Migración de clientes nuevos por lotes)
WHILE (@RowsAffected > 0 AND @TotalToLoad > 0)
BEGIN
    BEGIN TRY
        BEGIN TRAN;

        TRUNCATE TABLE #InsertedClients;

        -- 4.1 Insertar Clientes Maestro
        INSERT INTO operativo.Tbl_Maest_Cliente (
            Id_Tipo_Identificacion, Identificacion_Cliente, Nombre_Completo, 
            Fecha_Creacion, Usuario_Creacion, Esta_Verificado, Esta_Aprobado, 
            Esta_Eliminado, Esta_Anonimizado
        )
        OUTPUT inserted.Id_Cliente, inserted.Identificacion_Cliente INTO #InsertedClients
        SELECT TOP (@BatchSize) 
            @IdTipoIdentificacionCedula, 
            Identificacion, 
            NombreCompleto, 
            @FechaEjecucion, 
            @UserJob, 
            0, 0, 0, 0
        FROM #MigracionData;

        SET @RowsAffected = @@ROWCOUNT;

        IF @RowsAffected = 0
        BEGIN
            COMMIT TRAN;
            BREAK;
        END

        -- 4.2 Insertar Contactos (NOMBRES EN MINÚSCULAS Y TELEFONO6 INCLUIDO)
        INSERT INTO operativo.Tbl_Contacto_Cliente (
            Id_Cliente, Id_Tipo_Contacto, Valor_Contacto, Id_Estado_Verificacion, 
            Fecha_Creacion, Usuario_Creacion, Esta_Eliminado, Source, Id_Estado_LOPDP
        )
        SELECT DISTINCT 
            ic.Id_Cliente, 
            contacto.Id_Tipo_Contacto, 
            contacto.Valor, 
            @IdEstadoPendiente, 
            @FechaEjecucion, 
            @UserJob, 
            0, 
            @Source, 
            @IdEstadoPendienteLOPDP
        FROM #MigracionData src
        INNER JOIN #InsertedClients ic ON src.Identificacion = ic.Identificacion_Cliente
        CROSS APPLY (
            VALUES 
                -- ✅ Teléfonos de Contactabilidad (6 columnas con nombres correctos)
                (@IdTipoContactoCel, src.Telefono1), 
                (@IdTipoContactoCel, src.Telefono2), 
                (@IdTipoContactoCel, src.Telefono3), 
                (@IdTipoContactoCel, src.Telefono4), 
                (@IdTipoContactoCel, src.Telefono5),
                (@IdTipoContactoCel, src.Telefono6),  
                -- ✅ Teléfonos móviles (minúsculas)
                (@IdTipoContactoCel, src.telefono_movil1), 
                (@IdTipoContactoCel, src.telefono_movil2), 
                (@IdTipoContactoCel, src.telefono_movil3), 
                (@IdTipoContactoCel, src.telefono_movil4), 
                (@IdTipoContactoCel, src.telefono_movil5),
                -- ✅ Teléfonos fijos (minúsculas)
                (@IdTipoContactoTel, src.telefono_fijo1), 
                (@IdTipoContactoTel, src.telefono_fijo2), 
                (@IdTipoContactoTel, src.telefono_fijo3),
                (@IdTipoContactoTel, src.telefono_fijo4), 
                (@IdTipoContactoTel, src.telefono_fijo5),
                -- ✅ Emails
                (@IdTipoContactoEmail, src.Email1), 
                (@IdTipoContactoEmail, src.Email2), 
                (@IdTipoContactoEmail, src.Email3), 
                (@IdTipoContactoEmail, src.Email4), 
                (@IdTipoContactoEmail, src.Email5), 
                (@IdTipoContactoEmail, src.Email6)
        ) AS contacto(Id_Tipo_Contacto, Valor)
        WHERE contacto.Valor IS NOT NULL 
          AND LTRIM(RTRIM(contacto.Valor)) <> '';

        -- 4.3 Insertar Direcciones (minúsculas)
        INSERT INTO operativo.Tbl_Direccion_Cliente (
            Id_Cliente, Id_Tipo_Direccion, Direccion_Completa, Ciudad, Provincia, Pais, 
            Parroquia, Latitud, Longitud, Es_Principal, Estado_Verificacion, 
            Fecha_Creacion, Usuario_Creacion, Source_Direccion, estado_LOPDP, Esta_Eliminado
        )
        SELECT 
            ic.Id_Cliente, 
            @IdTipoDireccion, 
            dir.Direccion, 
            MAX(dir.Ciudad), 
            MAX(dir.Provincia), 
            @Pais, 
            MAX(dir.Parroquia), 
            MAX(dir.Latitud), 
            MAX(dir.Longitud), 
            MAX(dir.EsPrincipal), 
            @IdEstadoPendiente, 
            @FechaEjecucion, 
            @UserJob, 
            @Source,
            @IdEstadoPendiente, 
            0
        FROM #MigracionData src
        INNER JOIN #InsertedClients ic ON src.Identificacion = ic.Identificacion_Cliente
        CROSS APPLY (
            VALUES 
                (src.direccion1, src.ciudad1, NULL, NULL, NULL, NULL, 1),
                (src.direccion2, src.ciudad2, NULL, NULL, NULL, NULL, 0),
                (src.direccion3, src.ciudad3, NULL, NULL, NULL, NULL, 0),
                (src.direccion4, src.ciudad4, NULL, NULL, NULL, NULL, 0),
                (src.direccion5, src.ciudad5, NULL, NULL, NULL, NULL, 0),
                (src.direccion, src.canton, src.provincia, src.parroquia, src.latitud, src.longitud, 0)
        ) AS dir(Direccion, Ciudad, Provincia, Parroquia, Latitud, Longitud, EsPrincipal)
        WHERE dir.Direccion IS NOT NULL 
          AND LTRIM(RTRIM(dir.Direccion)) <> ''
        GROUP BY ic.Id_Cliente, dir.Direccion;

        DELETE m 
        FROM #MigracionData m 
        INNER JOIN #InsertedClients ic ON m.Identificacion = ic.Identificacion_Cliente;

        COMMIT TRAN;

        SET @TotalInserted = @TotalInserted + @RowsAffected;
        RAISERROR ('Lote procesado: %d clientes nuevos insertados. Total acumulado: %d', 10, 1, @RowsAffected, @TotalInserted) WITH NOWAIT;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;
        
        DECLARE @ErrorMsg NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorLn INT = ERROR_LINE();
        RAISERROR ('Error en lote de inserción (Línea %d): %s', 16, 1, @ErrorLn, @ErrorMsg);
        RETURN;
    END CATCH
END

PRINT '=======================================================';
PRINT 'MIGRACIÓN MASIVA COMPLETADA.';
PRINT 'Clientes Maestro Creados: ' + CAST(@TotalInserted AS VARCHAR);
PRINT 'Clientes Maestro Evaluados/Enriquecidos: ' + CAST(@TotalUpdated AS VARCHAR);
PRINT '=======================================================';

SELECT Identificacion, Motivo 
FROM #ReporteNoInsertados 
ORDER BY Motivo, Identificacion;

-- Limpieza final
DROP TABLE #MigracionRaw;
DROP TABLE #MigracionData;
DROP TABLE #ClientesExistentes;
DROP TABLE #InsertedClients;
DROP TABLE #ReporteNoInsertados;
IF OBJECT_ID('tempdb..#ContactosExistentes') IS NOT NULL DROP TABLE #ContactosExistentes;
IF OBJECT_ID('tempdb..#DireccionesExistentes') IS NOT NULL DROP TABLE #DireccionesExistentes;

PRINT 'Proceso finalizado correctamente.';