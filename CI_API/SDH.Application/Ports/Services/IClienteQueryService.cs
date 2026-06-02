using SDH.Application.DTOs.Clients;
using SDH.Domain.Entities.Operative;

namespace SDH.Application.Ports.Services
{
    public interface IClienteQueryService
    {
        Task<IReadOnlyList<CustomerSearchResultDto>> LightSearchByNameAndIdentificationAsync(string termino, int top, CancellationToken cancellationToken = default);
        Task<List<CustomerContactDto>> ObtenerContactosAsync(string cedula, CancellationToken cancellationToken = default);
        Task<List<CustomerAddressDto>> ObtenerDireccionesAsync(string cedula, CancellationToken cancellationToken = default);
        Task<CustomerAddressDto?> ObtenerDireccionPrincipalAsync(string cedula, CancellationToken cancellationToken = default);
        Task<Client?> ObtenerPorIdAsync(long id, CancellationToken cancellationToken = default);
        Task<CustomerDto?> ObtenerPorIdentificacionAsync(string identificacion, CancellationToken cancellationToken = default);
    }
}
