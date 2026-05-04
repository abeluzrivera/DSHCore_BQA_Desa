namespace SDH.Application.DTOs;

/// <summary>
/// DTO para mapear los resultados de la vista parametro.Vw_Cat_Detalle_General
/// Optimizado para consultas de alto rendimiento con caché
/// </summary>
/// <param name="CatalogItemId"></param>
/// <param name="CatalogGroupId"></param>
/// <param name="DisplayOrder"></param>
/// <param name="IsActive"></param>
/// <param name="IsSystem"></param>
public record CatalogDetail(int CatalogItemId,
                              int CatalogGroupId,
                              string GroupName,
                              string ValueCode,
                              string DisplayName,
                              int DisplayOrder,
                              bool IsActive,
                              bool IsSystem)
{
}
