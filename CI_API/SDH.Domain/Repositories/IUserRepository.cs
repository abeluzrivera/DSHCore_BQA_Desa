using SDH.Domain.Entities.Seguridad;

namespace SDH.Domain.Repositories
{
    /// <summary>
    /// Port (Interface) para el repositorio de Usuario
    /// </summary>
    public interface IUserRepository
    {
        Task<Users?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Users?> GetByUsernameAsync(string codigoUsuario, CancellationToken cancellationToken = default);
        Task<Users?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<IEnumerable<Users>> GetActiveAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Users>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Users> AddUserAsync(Users usuario, CancellationToken cancellationToken = default);
        Task UpdateUserAsync(Users usuario, CancellationToken cancellationToken = default);
        Task<bool> ExistsByUsernameAsync(string codigoUsuario, CancellationToken cancellationToken = default);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    }
}
