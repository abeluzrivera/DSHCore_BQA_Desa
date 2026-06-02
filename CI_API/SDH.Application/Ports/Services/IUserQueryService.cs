using SDH.Application.DTOs.Users;

namespace SDH.Application.Ports.Services
{
    /// <summary>
    /// CQRS Read Port para Usuario.
    /// Implementado en Infrastructure con AsNoTracking y proyección directa a DTO.
    /// </summary>
    public interface IUserQueryService
    {
        /// <summary>Lista todos los usuarios no eliminados, ordenados por nombre.</summary>
        Task<IReadOnlyList<UserListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>Retorna el detalle de un usuario por ID para el modal de edición. Null si no existe.</summary>
        Task<UserDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si ya existe un usuario con ese código.
        /// excludeId permite excluir el propio usuario en escenarios de actualización.
        /// </summary>
        Task<bool> ExistsByUsernameAsync(string codigoUsuario, int? excludeId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si ya existe un usuario con ese email.
        /// excludeId permite excluir el propio usuario en escenarios de actualización.
        /// </summary>
        Task<bool> ExistsByEmailAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default);
    }
}
