-- Script para actualizar el estado de verificación de clientes existentes
-- basándose en el estado de sus contactos

USE [ContactabilidadInteligente]; -- Ajustar el nombre de la base de datos según corresponda
GO

-- Actualizar clientes: marcar como verificado si TODOS sus contactos activos están verificados
UPDATE c
SET Esta_Verificado = CASE 
    WHEN EXISTS (
        -- Verificar si el cliente tiene contactos activos
        SELECT 1 
        FROM operativo.Tbl_Traz_Contacto_Cliente cc
        WHERE cc.Id_Cliente = c.Id_Cliente 
        AND cc.Esta_Eliminado = 0
    )
    AND NOT EXISTS (
        -- Verificar si existe algún contacto activo que NO esté verificado
        SELECT 1 
        FROM operativo.Tbl_Traz_Contacto_Cliente cc
        WHERE cc.Id_Cliente = c.Id_Cliente 
        AND cc.Esta_Eliminado = 0
        AND cc.Estado_Verificacion != 'VERIF'
    )
    THEN 1  -- Todos los contactos están verificados
    ELSE 0  -- Al menos un contacto no está verificado o no hay contactos
END
FROM operativo.Tbl_Maest_Cliente c;

-- Mostrar resumen de la actualización
SELECT 
    COUNT(*) AS Total_Clientes,
    SUM(CASE WHEN Esta_Verificado = 1 THEN 1 ELSE 0 END) AS Clientes_Verificados,
    SUM(CASE WHEN Esta_Verificado = 0 THEN 1 ELSE 0 END) AS Clientes_No_Verificados
FROM operativo.Tbl_Maest_Cliente;

-- Mostrar detalle de clientes por estado
SELECT 
    c.Identificacion_Cliente,
    c.Nombre_Completo,
    c.Esta_Verificado,
    COUNT(cc.Id_Contacto_Cliente) AS Total_Contactos,
    SUM(CASE WHEN cc.Estado_Verificacion = 'VERIF' THEN 1 ELSE 0 END) AS Contactos_Verificados,
    SUM(CASE WHEN cc.Estado_Verificacion = 'PEND' THEN 1 ELSE 0 END) AS Contactos_Pendientes
FROM operativo.Tbl_Maest_Cliente c
LEFT JOIN operativo.Tbl_Traz_Contacto_Cliente cc ON cc.Id_Cliente = c.Id_Cliente AND cc.Esta_Eliminado = 0
GROUP BY c.Identificacion_Cliente, c.Nombre_Completo, c.Esta_Verificado
ORDER BY c.Esta_Verificado DESC, c.Nombre_Completo;

GO
