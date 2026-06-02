SET NOCOUNT ON;

-- =================================================================
-- SCRIPT DE MIGRACIÓN MASIVA Y ENRIQUECIMIENTO - PRODUCCIÓN
-- Arquitectura: Integrado con Catálogos de Dominio (MDM)
-- =================================================================

DECLARE @BatchSize INT = 50000;
DECLARE @RowsAffected INT = 1;
DECLARE @TotalInserted INT = 0;
DECLARE @TotalToLoad INT = 0;
DECLARE @TotalUpdated INT = 0;
DECLARE @UserJob NVARCHAR(50) = 'SQLJOBMigrate';
DECLARE @Source NVARCHAR(15) = 'EQUIFAX';
DECLARE @Pais NVARCHAR(20) = 'ECUADOR';

PRINT '=======================================================';
PRINT 'PASO 0: Cargando IDs de Catálogos (Master Data Management)';
PRINT '=======================================================';

-- Variables para almacenar los IDs de los catálogos (Rendimiento O(1) en el bucle)
DECLARE @IdEstadoPendiente INT;
DECLARE @IdTipoContactoCel INT;
DECLARE @IdTipoContactoTel INT;
DECLARE @IdTipoContactoEmail INT;
DECLARE @IdTipoDireccion INT;
DECLARE @IdTipoIdentificacionCedula INT = 201;
DECLARE @IdEstadoPendienteLOPDP INT = 6;

-- Obtener IDs exactos cruzando con las tablas de parametrización
SELECT @IdEstadoPendiente = i.Id_Item_Catalogo FROM [parametro].[Tbl_Cat_Item] i INNER JOIN [parametro].[Tbl_Cat_Grupo] g ON i.Id_Grupo_Catalogo = g.Id_Grupo_Catalogo WHERE g.Nombre_Grupo = 'ESTADO_CONTACTABILIDAD' AND i.Codigo_Valor = 'PEND'; -- Ajusta 'PEND' al código real de tu enumerador
SELECT @IdTipoContactoCel = i.Id_Item_Catalogo FROM [parametro].[Tbl_Cat_Item] i INNER JOIN [parametro].[Tbl_Cat_Grupo] g ON i.Id_Grupo_Catalogo = g.Id_Grupo_Catalogo WHERE g.Nombre_Grupo = 'TIPO_CONTACTO' AND i.Codigo_Valor = 'CEL'; -- Ajusta 'CEL' a tu código real
SELECT @IdTipoContactoTel = i.Id_Item_Catalogo FROM [parametro].[Tbl_Cat_Item] i INNER JOIN [parametro].[Tbl_Cat_Grupo] g ON i.Id_Grupo_Catalogo = g.Id_Grupo_Catalogo WHERE g.Nombre_Grupo = 'TIPO_CONTACTO' AND i.Codigo_Valor = 'TEL_C';
SELECT @IdTipoContactoEmail = i.Id_Item_Catalogo FROM [parametro].[Tbl_Cat_Item] i INNER JOIN [parametro].[Tbl_Cat_Grupo] g ON i.Id_Grupo_Catalogo = g.Id_Grupo_Catalogo WHERE g.Nombre_Grupo = 'TIPO_CONTACTO' AND i.Codigo_Valor = 'EMAIL';
SELECT @IdTipoDireccion = i.Id_Item_Catalogo FROM [parametro].[Tbl_Cat_Item] i INNER JOIN [parametro].[Tbl_Cat_Grupo] g ON i.Id_Grupo_Catalogo = g.Id_Grupo_Catalogo WHERE g.Nombre_Grupo = 'TIPO_CONTACTO' AND i.Codigo_Valor = 'DIR_D';

print @IdEstadoPendiente;
print @IdTipoContactoCel;
print @IdTipoIdentificacionCedula;
print @IdTipoContactoTel;
print @IdTipoContactoEmail;
print @IdTipoDireccion;

-- Validador de Gobernanza: Abortar si faltan catálogos
IF (@IdEstadoPendiente IS NULL OR @IdTipoContactoCel IS NULL OR @IdTipoIdentificacionCedula IS NULL)
BEGIN
    
    RAISERROR ('Fallo de Integridad: No se encontraron los catálogos base. Ejecute el Seeder de EF Core primero.', 16, 1);
    RETURN;
END

PRINT '=======================================================';
PRINT 'PASO 1: Consolidando el universo de datos en TempDB...';
PRINT '=======================================================';

IF OBJECT_ID('tempdb..#MigracionRaw') IS NOT NULL DROP TABLE #MigracionRaw;
IF OBJECT_ID('tempdb..#MigracionData') IS NOT NULL DROP TABLE #MigracionData;
IF OBJECT_ID('tempdb..#ClientesExistentes') IS NOT NULL DROP TABLE #ClientesExistentes;
IF OBJECT_ID('tempdb..#InsertedClients') IS NOT NULL DROP TABLE #InsertedClients;
IF OBJECT_ID('tempdb..#ReporteNoInsertados') IS NOT NULL DROP TABLE #ReporteNoInsertados;

CREATE TABLE #ReporteNoInsertados (
    Identificacion VARCHAR(50),
    Motivo VARCHAR(100)
);

-- 1. UNIVERSO DE DATOS EN MEMORIA
WITH Contactabilidad AS (
    SELECT a.Identificacion, a.NombreCompleto, a.Telefono1, a.Telefono2, a.Telefono3, a.Telefono4, a.Telefono5, a.Email1, a.Email2, a.Email3, a.Email4, a.Email5, a.Email6
    FROM [DB_CONTACTABILIDAD].dbo.Tbl_Contactabilidad a
),
Direcciones AS (
    SELECT a.identificacion, a.nombre_sujeto AS Nombre_Completo, a.direccion1, a.direccion2, a.direccion3, a.direccion4, a.direccion5, a.telefono_fijo1, a.telefono_fijo2, a.telefono_fijo3, a.telefono_fijo4, a.telefono_fijo5, a.telefono_movil1, a.telefono_movil2, a.telefono_movil3, a.telefono_movil4, a.telefono_movil5, a.ciudad1, a.ciudad2, a.ciudad3, a.ciudad4, a.ciudad5
    FROM [DB_CONTACTABILIDAD].dbo.tbl_maestrodirecciones a
    WHERE a.direccion1 <> ''
),
GeoDireccion AS (
    SELECT a.identificacion, a.direccion, a.canton, a.provincia, a.parroquia, a.longitud, a.latitud
    FROM [DB_CONTACTABILIDAD].dbo.ScoreMiDireccion a
)
SELECT
    COALESCE(c.Identificacion, d.identificacion, g.identificacion) AS Identificacion,
    COALESCE(c.NombreCompleto, d.Nombre_Completo, '') AS NombreCompleto,
    c.Telefono1, c.Telefono2, c.Telefono3, c.Telefono4, c.Telefono5,
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
FROM #MigracionRaw GROUP BY Identificacion HAVING COUNT(*) > 1;

WITH CTE_Clean AS (
    SELECT *, ROW_NUMBER() OVER(PARTITION BY Identificacion ORDER BY Identificacion) as rn
    FROM #MigracionRaw
)
SELECT * INTO #MigracionData FROM CTE_Clean WHERE rn = 1;


PRINT 'PASO 3: Enriqueciendo Clientes Existentes...';

SELECT m.*, dest.Id_Cliente
INTO #ClientesExistentes
FROM #MigracionData m
INNER JOIN [operativo].[Tbl_Maest_Cliente] dest 
    ON m.Identificacion = dest.Identificacion_Cliente;

SELECT @TotalUpdated = @@ROWCOUNT;

IF @TotalUpdated > 0
BEGIN
    BEGIN TRAN;
    
    -- 3.2 Insertar Contactos Nuevos a Clientes Existentes (Usando variables de ID en lugar de strings)
    INSERT INTO [operativo].[Tbl_Contacto_Cliente] (
        [Id_Cliente], [Id_Tipo_Contacto], [Valor_Contacto], [Id_Estado_Verificacion], [Fecha_Creacion], [Usuario_Creacion], [Esta_Eliminado], [Source], Id_Estado_LOPDP
    )
    SELECT DISTINCT 
        ce.Id_Cliente, contacto.Id_Tipo_Contacto, contacto.Valor, @IdEstadoPendiente, GETDATE(), @UserJob, 0, @Source, @IdEstadoPendienteLOPDP
    FROM #ClientesExistentes ce
    CROSS APPLY (
        VALUES 
            (@IdTipoContactoCel, ce.Telefono1), (@IdTipoContactoCel, ce.Telefono2), (@IdTipoContactoCel, ce.Telefono3), (@IdTipoContactoCel, ce.Telefono4), (@IdTipoContactoCel, ce.Telefono5),
            (@IdTipoContactoCel, ce.TELEFONO_MOVIL1), (@IdTipoContactoCel, ce.TELEFONO_MOVIL2), (@IdTipoContactoCel, ce.TELEFONO_MOVIL3), (@IdTipoContactoCel, ce.TELEFONO_MOVIL4), (@IdTipoContactoCel, ce.TELEFONO_MOVIL5),
            (@IdTipoContactoTel, ce.TELEFONO_FIJO1), (@IdTipoContactoTel, ce.TELEFONO_FIJO2), (@IdTipoContactoTel, ce.TELEFONO_FIJO3),(@IdTipoContactoTel, ce.TELEFONO_FIJO4), (@IdTipoContactoTel, ce.TELEFONO_FIJO5),
            (@IdTipoContactoEmail, ce.Email1), (@IdTipoContactoEmail, ce.Email2), (@IdTipoContactoEmail, ce.Email3), (@IdTipoContactoEmail, ce.Email4), (@IdTipoContactoEmail, ce.Email5), (@IdTipoContactoEmail, ce.Email6)
    ) AS contacto(Id_Tipo_Contacto, Valor)
    WHERE contacto.Valor IS NOT NULL AND LTRIM(RTRIM(contacto.Valor)) <> ''
      AND NOT EXISTS (
          SELECT 1 FROM [operativo].[Tbl_Contacto_Cliente] tcc
          WHERE tcc.Id_Cliente = ce.Id_Cliente AND tcc.Valor_Contacto = contacto.Valor AND tcc.Id_Tipo_Contacto = contacto.Id_Tipo_Contacto
      );

    -- 3.3 Insertar Direcciones Nuevas a Clientes Existentes
    INSERT INTO [operativo].[Tbl_Direccion_Cliente] (
        [Id_Cliente], [Id_Tipo_Direccion], [Direccion_Completa], [Ciudad], [Provincia], [Pais], [Parroquia], [Latitud], [Longitud], [Es_Principal], Estado_Verificacion, [Fecha_Creacion], [Usuario_Creacion], [Source_Direccion], Esta_Eliminado
    )
    SELECT 
        ce.Id_Cliente, @IdTipoDireccion, dir.Direccion, MAX(dir.Ciudad), MAX(dir.Provincia), @Pais, MAX(dir.Parroquia), MAX(dir.Latitud), MAX(dir.Longitud), 0, @IdEstadoPendiente, GETDATE(), @UserJob, @Source, 0
    FROM #ClientesExistentes ce
    CROSS APPLY (
        VALUES 
            (ce.DIRECCION1, ce.CIUDAD1, NULL,          NULL,          NULL,         NULL),
            (ce.DIRECCION2, ce.CIUDAD2, NULL,          NULL,          NULL,         NULL),
            (ce.DIRECCION3, ce.CIUDAD3, NULL,          NULL,          NULL,         NULL),
            (ce.DIRECCION4, ce.CIUDAD4, NULL,          NULL,          NULL,         NULL),
            (ce.DIRECCION5, ce.CIUDAD5, NULL,          NULL,          NULL,         NULL),
            (ce.direccion,  ce.canton,  ce.provincia, ce.parroquia, ce.latitud,  ce.longitud)
    ) AS dir(Direccion, Ciudad, Provincia, Parroquia, Latitud, Longitud)
    WHERE dir.Direccion IS NOT NULL AND LTRIM(RTRIM(dir.Direccion)) <> ''
      AND NOT EXISTS (
          SELECT 1 FROM [operativo].[Tbl_Direccion_Cliente] tdc
          WHERE tdc.Id_Cliente = ce.Id_Cliente AND tdc.Direccion_Completa = dir.Direccion
      )
    GROUP BY ce.Id_Cliente, dir.Direccion;

    COMMIT TRAN;

    INSERT INTO #ReporteNoInsertados (Identificacion, Motivo)
    SELECT Identificacion, 'Ya existía. Se evaluó y enriqueció sus contactos/direcciones'
    FROM #ClientesExistentes;

    DELETE m FROM #MigracionData m INNER JOIN #ClientesExistentes ce ON m.Identificacion = ce.Identificacion;
    
    PRINT 'Se enriquecieron ' + CAST(@TotalUpdated AS VARCHAR) + ' clientes existentes.';
END


SELECT @TotalToLoad = COUNT(*) FROM #MigracionData;

CREATE CLUSTERED INDEX IX_MigracionData_Identificacion ON #MigracionData(Identificacion);
CREATE TABLE #InsertedClients (Id_Cliente BIGINT, Identificacion_Cliente VARCHAR(50));

PRINT '=======================================================';
PRINT 'Total de clientes 100% NUEVOS a insertar: ' + CAST(@TotalToLoad AS VARCHAR);
PRINT 'Iniciando migración en lotes de ' + CAST(@BatchSize AS VARCHAR) + '...';
PRINT '=======================================================';

-- 4. BUCLE PRINCIPAL (Migración de clientes nuevos por lotes)
WHILE (@RowsAffected > 0 AND @TotalToLoad > 0)
BEGIN
    BEGIN TRAN;

    TRUNCATE TABLE #InsertedClients;

    -- 4.1 Insertar Clientes (Padre) - Agregado Id_Tipo_Identificacion
    INSERT INTO [operativo].[Tbl_Maest_Cliente] (
        [Id_Tipo_Identificacion], [Identificacion_Cliente], [Nombre_Completo], [Fecha_Creacion], [Usuario_Creacion], [Esta_Verificado], [Esta_Aprobado], [Esta_Eliminado], Esta_Anonimizado
    )
    OUTPUT inserted.Id_Cliente, inserted.Identificacion_Cliente INTO #InsertedClients
    SELECT TOP (@BatchSize) 
        @IdTipoIdentificacionCedula, [Identificacion], [NombreCompleto], GETDATE(), @UserJob, 0, 0, 0,0
    FROM #MigracionData;

    SET @RowsAffected = @@ROWCOUNT;

    IF @RowsAffected = 0
    BEGIN
        COMMIT TRAN;
        BREAK;
    END

    -- 4.2 Insertar Contactos (Hijo)
    INSERT INTO [operativo].[Tbl_Contacto_Cliente] (
        [Id_Cliente], [Id_Tipo_Contacto], [Valor_Contacto], [Id_Estado_Verificacion], [Fecha_Creacion], [Usuario_Creacion], [Esta_Eliminado], [Source], Id_Estado_LOPDP
    )
    SELECT DISTINCT 
        ic.Id_Cliente, contacto.Id_Tipo_Contacto, contacto.Valor, @IdEstadoPendiente, GETDATE(), @UserJob, 0, @Source, @IdEstadoPendienteLOPDP
    FROM #MigracionData src
    INNER JOIN #InsertedClients ic ON src.Identificacion = ic.Identificacion_Cliente
    CROSS APPLY (
        VALUES 
            (@IdTipoContactoCel, src.Telefono1), (@IdTipoContactoCel, src.Telefono2), (@IdTipoContactoCel, src.Telefono3), (@IdTipoContactoCel, src.Telefono4), (@IdTipoContactoCel, src.Telefono5),
            (@IdTipoContactoCel, src.TELEFONO_MOVIL1), (@IdTipoContactoCel, src.TELEFONO_MOVIL2), (@IdTipoContactoCel, src.TELEFONO_MOVIL3), (@IdTipoContactoCel, src.TELEFONO_MOVIL4), (@IdTipoContactoCel, src.TELEFONO_MOVIL5),
            (@IdTipoContactoTel, src.TELEFONO_FIJO1), (@IdTipoContactoTel, src.TELEFONO_FIJO2), (@IdTipoContactoTel, src.TELEFONO_FIJO3),(@IdTipoContactoTel, src.TELEFONO_FIJO4), (@IdTipoContactoTel, src.TELEFONO_FIJO5),
            (@IdTipoContactoEmail, src.Email1), (@IdTipoContactoEmail, src.Email2), (@IdTipoContactoEmail, src.Email3), (@IdTipoContactoEmail, src.Email4), (@IdTipoContactoEmail, src.Email5), (@IdTipoContactoEmail, src.Email6)
    ) AS contacto(Id_Tipo_Contacto, Valor)
    WHERE contacto.Valor IS NOT NULL AND LTRIM(RTRIM(contacto.Valor)) <> '';

    -- 4.3 Insertar Direcciones (Hijo)
    INSERT INTO [operativo].[Tbl_Direccion_Cliente] (
        [Id_Cliente], [Id_Tipo_Direccion], [Direccion_Completa], [Ciudad], [Provincia], [Pais], [Parroquia], [Latitud], [Longitud], [Es_Principal], Estado_Verificacion, [Fecha_Creacion], [Usuario_Creacion], [Source_Direccion], estado_LOPDP, [Esta_Eliminado]
    )
    SELECT 
        ic.Id_Cliente, @IdTipoDireccion, dir.Direccion, MAX(dir.Ciudad), MAX(dir.Provincia), @Pais, MAX(dir.Parroquia), MAX(dir.Latitud), MAX(dir.Longitud), MAX(dir.EsPrincipal), @IdEstadoPendiente, GETDATE(), @UserJob, @Source,@IdEstadoPendiente, 0
    FROM #MigracionData src
    INNER JOIN #InsertedClients ic ON src.Identificacion = ic.Identificacion_Cliente
    CROSS APPLY (
        VALUES 
            (src.DIRECCION1, src.CIUDAD1, NULL,          NULL,          NULL,         NULL,          1),
            (src.DIRECCION2, src.CIUDAD2, NULL,          NULL,          NULL,         NULL,          0),
            (src.DIRECCION3, src.CIUDAD3, NULL,          NULL,          NULL,         NULL,          0),
            (src.DIRECCION4, src.CIUDAD4, NULL,          NULL,          NULL,         NULL,          0),
            (src.DIRECCION5, src.CIUDAD5, NULL,          NULL,          NULL,         NULL,          0),
            (src.direccion,  src.canton,  src.provincia, src.parroquia, src.latitud,  src.longitud,  0)
    ) AS dir(Direccion, Ciudad, Provincia, Parroquia, Latitud, Longitud, EsPrincipal)
    WHERE dir.Direccion IS NOT NULL AND LTRIM(RTRIM(dir.Direccion)) <> ''
    GROUP BY ic.Id_Cliente, dir.Direccion;

    DELETE m FROM #MigracionData m INNER JOIN #InsertedClients ic ON m.Identificacion = ic.Identificacion_Cliente;

    COMMIT TRAN;

    SET @TotalInserted = @TotalInserted + @RowsAffected;
    RAISERROR ('Lote procesado: %d clientes nuevos insertados. Total acumulado: %d', 10, 1, @RowsAffected, @TotalInserted) WITH NOWAIT;
    
END

PRINT '=======================================================';
PRINT 'MIGRACIÓN MASIVA COMPLETADA.';
PRINT 'Clientes Maestro Creados: ' + CAST(@TotalInserted AS VARCHAR);
PRINT 'Clientes Maestro Evaluados/Enriquecidos: ' + CAST(@TotalUpdated AS VARCHAR);
PRINT '=======================================================';

SELECT Identificacion, Motivo FROM #ReporteNoInsertados ORDER BY Motivo, Identificacion;

DROP TABLE #MigracionRaw;
DROP TABLE #MigracionData;
DROP TABLE #ClientesExistentes;
DROP TABLE #InsertedClients;
DROP TABLE #ReporteNoInsertados;