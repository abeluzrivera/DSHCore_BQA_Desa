using SDH.Domain.Entities.Operative;

namespace SDH.Domain.Repositories
{
    /// <summary>
    /// Port (Interface) para el repositorio de Cliente siguiendo el patrón de Arquitectura Hexagonal
    /// </summary>
    public interface ICustomerRepository
    {
        Task<Client?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<Client> AddCustomerAsync(Client cliente, CancellationToken cancellationToken = default);

        Task UpdateCustomerAsync(Client cliente, CancellationToken cancellationToken = default);
        Task<bool> ExistsByIdentificationAsync(string identificacion, CancellationToken cancellationToken = default);
        Task<Client?> GetByIdentificationAsync(string identificacion, CancellationToken cancellationToken = default);
    }
}
