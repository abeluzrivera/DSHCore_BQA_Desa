// Asegúrate de usar el namespace donde guardaste el record CatalogoDetalle
using SDH.Application.DTOs;

namespace SDH.Application.Ports.Queries
{
    /// <summary>
    /// LADO CONSULTAS (Lectura): Port para leer catálogos a máxima velocidad.
    /// Utiliza la vista de base de datos y devuelve DTOs (Records).
    /// </summary>
    public interface ICatalogQueryService
    {
        // Métodos optimizados que leen directo de Vw_Cat_Detalle_General
        Task<IReadOnlyList<CatalogDetail>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<CatalogDetail>> GetActiveAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<CatalogDetail>> GetByGroupNameAsync(string nombreGrupo, CancellationToken cancellationToken = default);
        Task<CatalogDetail?> GetItemByCodeAsync(string nombreGrupo, string codigoValor, CancellationToken cancellationToken = default);

        //Validacion de catalogos
        Task<List<CatalogDiscrepancyDto>> CheckIntegrityAsync(CancellationToken ct = default);
    }
}