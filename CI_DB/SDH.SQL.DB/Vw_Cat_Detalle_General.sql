CREATE OR ALTER VIEW [parametro].[Vw_Cat_Detalle_General]
AS
SELECT
    i.Id_Item_Catalogo,
    g.Id_Grupo_Catalogo,
    g.Nombre_Grupo,
    i.Codigo_Valor,
    i.Texto_Visual,
    i.Orden_Visual,
    i.Esta_Activo,
    g.Es_Sistema
FROM parametro.Tbl_Cat_Item  AS i
JOIN parametro.Tbl_Cat_Grupo AS g ON g.Id_Grupo_Catalogo = i.Id_Grupo_Catalogo;
