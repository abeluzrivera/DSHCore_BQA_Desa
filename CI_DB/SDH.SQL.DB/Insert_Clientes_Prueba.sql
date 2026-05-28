-- ============================================================
-- Script de datos de prueba: 3 clientes con 5 datos de
-- contactabilidad cada uno (2 teléfonos, 2 correos, 1 dirección)
-- ============================================================

USE [DB_ODS_DEV1];
SET NOCOUNT ON;
SET XACT_ABORT ON;


DECLARE @IdEstadoPendiente          INT;
DECLARE @IdTipoContactoCel          INT;
DECLARE @IdTipoContactoTel          INT;
DECLARE @IdTipoContactoEmail        INT;
DECLARE @IdTipoDireccion            INT;
DECLARE @IdTipoIdentificacionCedula INT;
DECLARE @IdEstadoPendienteLOPDP     INT;

SELECT
    @IdEstadoPendiente          = MAX(CASE WHEN g.Nombre_Grupo = 'ESTADO_CONTACTABILIDAD' AND i.Codigo_Valor = 'PEND'      THEN i.Id_Item_Catalogo END),
    @IdTipoContactoCel          = MAX(CASE WHEN g.Nombre_Grupo = 'TIPO_CONTACTO'          AND i.Codigo_Valor = 'CEL'       THEN i.Id_Item_Catalogo END),
    @IdTipoContactoTel          = MAX(CASE WHEN g.Nombre_Grupo = 'TIPO_CONTACTO'          AND i.Codigo_Valor = 'TEL_C'     THEN i.Id_Item_Catalogo END),
    @IdTipoContactoEmail        = MAX(CASE WHEN g.Nombre_Grupo = 'TIPO_CONTACTO'          AND i.Codigo_Valor = 'EMAIL'     THEN i.Id_Item_Catalogo END),
    @IdTipoDireccion            = MAX(CASE WHEN g.Nombre_Grupo = 'TIPO_CONTACTO'          AND i.Codigo_Valor = 'DIR_D'     THEN i.Id_Item_Catalogo END),
    @IdTipoIdentificacionCedula = MAX(CASE WHEN g.Nombre_Grupo = 'TIPO_IDENTIFICACION'    AND i.Codigo_Valor = 'DNI'       THEN i.Id_Item_Catalogo END),
    @IdEstadoPendienteLOPDP     = MAX(CASE WHEN g.Nombre_Grupo = 'ESTADO_CONTACTABILIDAD' AND i.Codigo_Valor = 'PEND-LOPDP' THEN i.Id_Item_Catalogo END)
FROM parametro.Tbl_Cat_Item  i
INNER JOIN parametro.Tbl_Cat_Grupo g ON i.Id_Grupo_Catalogo = g.Id_Grupo_Catalogo
WHERE (g.Nombre_Grupo = 'ESTADO_CONTACTABILIDAD' AND i.Codigo_Valor IN ('PEND', 'PEND-LOPDP'))
   OR (g.Nombre_Grupo = 'TIPO_CONTACTO'          AND i.Codigo_Valor IN ('CEL', 'TEL_C', 'EMAIL', 'DIR_D'))
   OR (g.Nombre_Grupo = 'TIPO_IDENTIFICACION'    AND i.Codigo_Valor = 'DNI');

PRINT CONCAT('IDs resueltos — DNI=',      @IdTipoIdentificacionCedula,
             ', Estado=',                  @IdEstadoPendiente,
             ', LOPDP=',                   @IdEstadoPendienteLOPDP,
             ', Cel=',                     @IdTipoContactoCel,
             ', Tel=',                     @IdTipoContactoTel,
             ', Email=',                   @IdTipoContactoEmail,
             ', Dir=',                     @IdTipoDireccion);

IF @IdEstadoPendiente IS NULL OR @IdTipoContactoCel IS NULL
   OR @IdTipoContactoEmail IS NULL OR @IdTipoDireccion IS NULL
   OR @IdTipoIdentificacionCedula IS NULL OR @IdEstadoPendienteLOPDP IS NULL
BEGIN
    PRINT '--- Diagnóstico: grupos disponibles en la BD ---';
    SELECT g.Nombre_Grupo, COUNT(*) AS Total_Items
    FROM parametro.Tbl_Cat_Grupo g
    LEFT JOIN parametro.Tbl_Cat_Item i ON g.Id_Grupo_Catalogo = i.Id_Grupo_Catalogo
    GROUP BY g.Nombre_Grupo
    ORDER BY g.Nombre_Grupo;

    RAISERROR('Fallo de Integridad: No se encontraron los catálogos base. Ejecute el Seeder de EF Core primero iniciando la aplicación.', 16, 1);
    RETURN;
END

IF OBJECT_ID('tempdb..#PruebaClientes') IS NOT NULL DROP TABLE #PruebaClientes;
IF OBJECT_ID('tempdb..#InsertedClients') IS NOT NULL DROP TABLE #InsertedClients;

CREATE TABLE #PruebaClientes (
    Identificacion NVARCHAR(20),
    NombreCompleto NVARCHAR(250),
    Telefono1      NVARCHAR(50), Telefono2 NVARCHAR(50),
    Email1         NVARCHAR(100), Email2   NVARCHAR(100),
    Direccion1     NVARCHAR(500), Ciudad1  NVARCHAR(100)
);

INSERT INTO #PruebaClientes VALUES
    ('1701234567', 'Carlos Andrade Mora',   '0987654321', '022345678', 'carlos.andrade@gmail.com',  'candrade@empresa.com',   'Av. Amazonas N35-17 y Japón',      'Quito'),
    ('1756789012', 'María Fernández Reyes', '0991234567', '023456789', 'maria.fernandez@gmail.com', 'mfernandez@empresa.com', 'Calle Olmedo 234 y García Moreno', 'Guayaquil'),
    ('1798765432', 'Roberto Guzmán Soto',   '0976543210', '024567890', 'roberto.guzman@gmail.com',  'rguzman@empresa.com',    'Bolívar 156 y Sucre',              'Cuenca');

DELETE p FROM #PruebaClientes p
INNER JOIN operativo.Tbl_Maest_Cliente c ON c.Identificacion_Cliente = p.Identificacion AND c.Esta_Eliminado = 0;

IF NOT EXISTS (SELECT 1 FROM #PruebaClientes)
BEGIN
    PRINT 'Los 3 clientes de prueba ya existen en la base de datos. No se insertó nada.';
    RETURN;
END

CREATE TABLE #InsertedClients (
    Id_Cliente             BIGINT,
    Identificacion_Cliente NVARCHAR(20)
);

BEGIN TRY
    BEGIN TRAN;

    INSERT INTO operativo.Tbl_Maest_Cliente (
        Id_Tipo_Identificacion, Identificacion_Cliente, Nombre_Completo,
        Fecha_Creacion, Usuario_Creacion,
        Esta_Verificado, Esta_Aprobado, Esta_Eliminado, Esta_Anonimizado
    )
    OUTPUT inserted.Id_Cliente, inserted.Identificacion_Cliente INTO #InsertedClients
    SELECT @IdTipoIdentificacionCedula, Identificacion, NombreCompleto,
           SYSDATETIME(), 'SEED', 0, 0, 0, 0
    FROM #PruebaClientes;

    PRINT CONCAT('Clientes insertados: ', @@ROWCOUNT);

    INSERT INTO operativo.Tbl_Contacto_Cliente (
        Id_Cliente, Id_Tipo_Contacto, Valor_Contacto,
        Id_Estado_Verificacion, Fecha_Creacion, Usuario_Creacion,
        Esta_Eliminado, Source, Id_Estado_LOPDP
    )
    SELECT DISTINCT
        ic.Id_Cliente, contacto.Tipo, contacto.Valor,
        @IdEstadoPendiente, SYSDATETIME(), 'SEED', 0, 'MANUAL', @IdEstadoPendienteLOPDP
    FROM #PruebaClientes src
    INNER JOIN #InsertedClients ic ON src.Identificacion = ic.Identificacion_Cliente
    CROSS APPLY (
        VALUES
            (@IdTipoContactoCel,   src.Telefono1),
            (@IdTipoContactoCel,   src.Telefono2),
            (@IdTipoContactoEmail, src.Email1),
            (@IdTipoContactoEmail, src.Email2)
    ) AS contacto(Tipo, Valor)
    WHERE contacto.Valor IS NOT NULL AND LTRIM(RTRIM(contacto.Valor)) <> '';

    PRINT CONCAT('Contactos insertados: ', @@ROWCOUNT);

    INSERT INTO operativo.Tbl_Direccion_Cliente (
        Id_Cliente, Id_Tipo_Direccion, Direccion_Completa, Ciudad,
        Pais, Es_Principal, Estado_Verificacion,
        Fecha_Creacion, Usuario_Creacion, Source_Direccion, Esta_Eliminado
    )
    SELECT ic.Id_Cliente, @IdTipoDireccion, dir.Direccion, MAX(dir.Ciudad),
           'ECUADOR', 1, @IdEstadoPendiente,
           SYSDATETIME(), 'SEED', 'MANUAL', 0
    FROM #PruebaClientes src
    INNER JOIN #InsertedClients ic ON src.Identificacion = ic.Identificacion_Cliente
    CROSS APPLY (VALUES (src.Direccion1, src.Ciudad1)) AS dir(Direccion, Ciudad)
    WHERE dir.Direccion IS NOT NULL AND LTRIM(RTRIM(dir.Direccion)) <> ''
    GROUP BY ic.Id_Cliente, dir.Direccion;

    PRINT CONCAT('Direcciones insertadas: ', @@ROWCOUNT);

    COMMIT TRAN;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRAN;
    RAISERROR('Error (Línea %d): %s', 16, 1);
    RETURN;
END CATCH

-- ============================================================
-- Verificación
-- ============================================================
SELECT c.Nombre_Completo, cat.Texto_Visual AS Tipo, con.Valor_Contacto
FROM operativo.Tbl_Contacto_Cliente con
JOIN operativo.Tbl_Maest_Cliente    c   ON c.Id_Cliente        = con.Id_Cliente
JOIN parametro.Tbl_Cat_Item         cat ON cat.Id_Item_Catalogo = con.Id_Tipo_Contacto
WHERE c.Identificacion_Cliente IN ('1701234567','1756789012','1798765432')
ORDER BY c.Nombre_Completo, cat.Orden_Visual;

SELECT c.Nombre_Completo, d.Direccion_Completa, d.Ciudad
FROM operativo.Tbl_Direccion_Cliente d
JOIN operativo.Tbl_Maest_Cliente     c ON c.Id_Cliente = d.Id_Cliente
WHERE c.Identificacion_Cliente IN ('1701234567','1756789012','1798765432');

DROP TABLE #PruebaClientes;
DROP TABLE #InsertedClients;
