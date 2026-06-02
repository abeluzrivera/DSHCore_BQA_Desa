 SET NUMERIC_ROUNDABORT OFF;
SET ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, ARITHABORT,
    QUOTED_IDENTIFIER, ANSI_NULLS ON;
GO

CREATE VIEW parametro.Vw_Cat_Detalle_General
WITH SCHEMABINDING 
AS
SELECT 
    i.Id_Item_Catalogo,
    i.Id_Grupo_Catalogo,
    g.Nombre_Grupo,
    i.Codigo_Valor,
    i.Texto_Visual,
    i.Orden_Visual,
    i.Esta_Activo,
    g.Es_Sistema
FROM parametro.Tbl_Cat_Item i
INNER JOIN parametro.Tbl_Cat_Grupo g ON i.Id_Grupo_Catalogo = g.Id_Grupo_Catalogo;
GO