using SDH.Application.DTOs.Clients;
using SDH.Application.Models;
using SDH.Domain.Enums.LogicaNegocio;

namespace SDH.Application.Ports.Services
{
    public interface ICustomerCacheService
    {
        Task<IReadOnlyList<CustomerSearchResultDto>> SearchByNameAndIdentificationAsync(string termino, CancellationToken cancellationToken = default);
        Task<CustomerCacheModel?> GetSegmentedDetailAsync(string cedula, EnumQueryClientType tipo, CancellationToken cancellationToken = default);
        Task<CustomerCacheModel?> GetOrHydrateFullProfileAsync(string cedula, CancellationToken ct = default);
        /// <summary>
        /// Re-fetches y actualiza quirurgicamente un unico segmento (Contact o Address) tras una operacion de escritura.
        /// Preserva el resto del cliente en cache. No-op si el cliente no esta en cache.
        /// </summary>
        Task RefreshSegmentAsync(string identification, EnumQueryClientType tipo, CancellationToken cancellationToken = default);
        /// <summary>Invalida completamente la entrada de cache. Preferir RefreshSegmentAsync para cambios de contactabilidad.</summary>
        Task InvalidateClienteAsync(string cedula);
    }
}
