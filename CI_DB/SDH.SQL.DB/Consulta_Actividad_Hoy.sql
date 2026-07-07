USE [DB_ODS]
GO

-- =============================================
-- Consulta de actividad del dia actual
-- Misma logica que SP_Reporte_Actividad_Diaria
-- pero contra las tablas fuente directamente.
-- Usar cuando el job nocturno aun no proceso hoy.
-- =============================================

DECLARE @Hoy          date = CAST(GETDATE() AS date);
DECLARE @FechaMigracion date = (SELECT Fecha_Migracion FROM reportes.Tbl_Baseline_Carga_Inicial);

PRINT 'Fecha consultada: ' + CONVERT(varchar, @Hoy);
PRINT 'Fecha migracion:  ' + CONVERT(varchar, @FechaMigracion);

-- =============================================
-- 1. RESUMEN DEL DIA (equivalente a Tbl_Sum)
-- =============================================
SELECT
    'RESUMEN' AS Seccion,
    Tipo_Entidad,
    Tipo_Accion,
    Texto_Tipo,
    Usuario_Responsable,
    COUNT(*) AS Total
FROM (

    -- Contactos confirmados hoy
    SELECT
        'CONTACTO'   AS Tipo_Entidad,
        'CONFIRMADO' AS Tipo_Accion,
        CASE Id_Tipo_Contacto
            WHEN 101 THEN 'Celular'
            WHEN 102 THEN 'Correo'
            WHEN 103 THEN 'Telefono Convencional'
            ELSE 'Otro'
        END AS Texto_Tipo,
        Usuario_Verificador AS Usuario_Responsable
    FROM operativo.Tbl_Contacto_Cliente
    WHERE CAST(Fecha_Verificacion AS date) = @Hoy
      AND Esta_Eliminado = 0

    UNION ALL

    -- Contactos agregados hoy
    SELECT
        'CONTACTO',
        'AGREGADO',
        CASE Id_Tipo_Contacto
            WHEN 101 THEN 'Celular'
            WHEN 102 THEN 'Correo'
            WHEN 103 THEN 'Telefono Convencional'
            ELSE 'Otro'
        END,
        Usuario_Creacion
    FROM operativo.Tbl_Contacto_Cliente
    WHERE CAST(Fecha_Creacion AS date) = @Hoy
      AND CAST(Fecha_Creacion AS date) <> @FechaMigracion
      AND Esta_Eliminado = 0

    UNION ALL

    -- Direcciones confirmadas hoy
    SELECT
        'DIRECCION',
        'CONFIRMADO',
        CASE Id_Tipo_Direccion
            WHEN 105 THEN 'Domicilio'
            WHEN 106 THEN 'Trabajo'
            ELSE 'Otro'
        END,
        Usuario_Verificador
    FROM operativo.Tbl_Direccion_Cliente
    WHERE CAST(Fecha_Verificacion AS date) = @Hoy
      AND Esta_Eliminado = 0

    UNION ALL

    -- Direcciones agregadas hoy
    SELECT
        'DIRECCION',
        'AGREGADO',
        CASE Id_Tipo_Direccion
            WHEN 105 THEN 'Domicilio'
            WHEN 106 THEN 'Trabajo'
            ELSE 'Otro'
        END,
        Usuario_Creacion
    FROM operativo.Tbl_Direccion_Cliente
    WHERE CAST(Fecha_Creacion AS date) = @Hoy
      AND CAST(Fecha_Creacion AS date) <> @FechaMigracion
      AND Esta_Eliminado = 0

) AS Actividad
GROUP BY Tipo_Entidad, Tipo_Accion, Texto_Tipo, Usuario_Responsable
ORDER BY Tipo_Entidad, Tipo_Accion, Texto_Tipo, Usuario_Responsable;

-- =============================================
-- 2. DETALLE DEL DIA (equivalente a Tbl_Det)
-- =============================================
SELECT
    'DETALLE'    AS Seccion,
    Tipo_Entidad,
    Tipo_Accion,
    Texto_Tipo,
    Id_Entidad,
    Id_Cliente,
    Valor,
    Usuario_Responsable,
    Source,
    Fecha_Accion
FROM (

    SELECT
        'CONTACTO'   AS Tipo_Entidad,
        'CONFIRMADO' AS Tipo_Accion,
        CASE Id_Tipo_Contacto
            WHEN 101 THEN 'Celular'
            WHEN 102 THEN 'Correo'
            WHEN 103 THEN 'Telefono Convencional'
            ELSE 'Otro'
        END AS Texto_Tipo,
        Id_Contacto_Cliente AS Id_Entidad,
        Id_Cliente,
        Valor_Contacto      AS Valor,
        Usuario_Verificador AS Usuario_Responsable,
        Source,
        Fecha_Verificacion  AS Fecha_Accion
    FROM operativo.Tbl_Contacto_Cliente
    WHERE CAST(Fecha_Verificacion AS date) = @Hoy
      AND Esta_Eliminado = 0

    UNION ALL

    SELECT
        'CONTACTO',
        'AGREGADO',
        CASE Id_Tipo_Contacto
            WHEN 101 THEN 'Celular'
            WHEN 102 THEN 'Correo'
            WHEN 103 THEN 'Telefono Convencional'
            ELSE 'Otro'
        END,
        Id_Contacto_Cliente,
        Id_Cliente,
        Valor_Contacto,
        Usuario_Creacion,
        Source,
        Fecha_Creacion
    FROM operativo.Tbl_Contacto_Cliente
    WHERE CAST(Fecha_Creacion AS date) = @Hoy
      AND CAST(Fecha_Creacion AS date) <> @FechaMigracion
      AND Esta_Eliminado = 0

    UNION ALL

    SELECT
        'DIRECCION',
        'CONFIRMADO',
        CASE Id_Tipo_Direccion
            WHEN 105 THEN 'Domicilio'
            WHEN 106 THEN 'Trabajo'
            ELSE 'Otro'
        END,
        Id_Direccion_Cliente,
        Id_Cliente,
        Direccion_Completa,
        Usuario_Verificador,
        Source_Direccion,
        Fecha_Verificacion
    FROM operativo.Tbl_Direccion_Cliente
    WHERE CAST(Fecha_Verificacion AS date) = @Hoy
      AND Esta_Eliminado = 0

    UNION ALL

    SELECT
        'DIRECCION',
        'AGREGADO',
        CASE Id_Tipo_Direccion
            WHEN 105 THEN 'Domicilio'
            WHEN 106 THEN 'Trabajo'
            ELSE 'Otro'
        END,
        Id_Direccion_Cliente,
        Id_Cliente,
        Direccion_Completa,
        Usuario_Creacion,
        Source_Direccion,
        Fecha_Creacion
    FROM operativo.Tbl_Direccion_Cliente
    WHERE CAST(Fecha_Creacion AS date) = @Hoy
      AND CAST(Fecha_Creacion AS date) <> @FechaMigracion
      AND Esta_Eliminado = 0

) AS Detalle
ORDER BY Tipo_Entidad, Tipo_Accion, Fecha_Accion DESC;

-- =============================================
-- 3. TOTALES RAPIDOS
-- =============================================
SELECT
    SUM(CASE WHEN Tipo_Entidad = 'CONTACTO'  AND Tipo_Accion = 'CONFIRMADO' THEN Total ELSE 0 END) AS Contactos_Confirmados,
    SUM(CASE WHEN Tipo_Entidad = 'CONTACTO'  AND Tipo_Accion = 'AGREGADO'   THEN Total ELSE 0 END) AS Contactos_Agregados,
    SUM(CASE WHEN Tipo_Entidad = 'DIRECCION' AND Tipo_Accion = 'CONFIRMADO' THEN Total ELSE 0 END) AS Direcciones_Confirmadas,
    SUM(CASE WHEN Tipo_Entidad = 'DIRECCION' AND Tipo_Accion = 'AGREGADO'   THEN Total ELSE 0 END) AS Direcciones_Agregadas
FROM (
    SELECT Tipo_Entidad, Tipo_Accion, COUNT(*) AS Total
    FROM (
        SELECT 'CONTACTO' AS Tipo_Entidad, 'CONFIRMADO' AS Tipo_Accion
        FROM operativo.Tbl_Contacto_Cliente
        WHERE CAST(Fecha_Verificacion AS date) = @Hoy AND Esta_Eliminado = 0
        UNION ALL
        SELECT 'CONTACTO', 'AGREGADO'
        FROM operativo.Tbl_Contacto_Cliente
        WHERE CAST(Fecha_Creacion AS date) = @Hoy AND CAST(Fecha_Creacion AS date) <> @FechaMigracion AND Esta_Eliminado = 0
        UNION ALL
        SELECT 'DIRECCION', 'CONFIRMADO'
        FROM operativo.Tbl_Direccion_Cliente
        WHERE CAST(Fecha_Verificacion AS date) = @Hoy AND Esta_Eliminado = 0
        UNION ALL
        SELECT 'DIRECCION', 'AGREGADO'
        FROM operativo.Tbl_Direccion_Cliente
        WHERE CAST(Fecha_Creacion AS date) = @Hoy AND CAST(Fecha_Creacion AS date) <> @FechaMigracion AND Esta_Eliminado = 0
    ) AS Base
    GROUP BY Tipo_Entidad, Tipo_Accion
) AS Resumen;
