using Microsoft.EntityFrameworkCore;
using SDH.Application.DTOs.Users;
using SDH.Application.Ports.Services;
using SDH.infrastructure.Persistence.Data;

namespace SDH.infrastructure.Persistence.Queries
{
    /// <summary>
    /// CQRS Read Service para Usuario.
    /// Proyecta directamente a DTOs con AsNoTracking para maxima performance.
    /// </summary>
    public class UserQueryService(ApplicationDbContext context) : IUserQueryService
    {

        public async Task<IReadOnlyList<UserListItemDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return await context.Users
                .AsNoTracking()
                .Where(u => !u.IsDeleted)
                .OrderBy(u => u.FullName)
                .Select(u => new UserListItemDto(
                    u.Id,
                    u.FullName,
                    u.Email,
                    u.Username,
                    u.SystemRole,
                    u.IsActive,
                    u.IsLockedOut ?? false,
                    u.LastLoginAt))
                .ToListAsync(cancellationToken);
        }

        public async Task<UserDetailDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await context.Users
                .AsNoTracking()
                .Where(u => u.Id == id && !u.IsDeleted)
                .Select(u => new UserDetailDto(
                    u.Id,
                    u.FullName,
                    u.Username,
                    u.Email,
                    u.SystemRole,
                    u.IsActive))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> ExistsByUsernameAsync(
            string codigoUsuario,
            int? excludeId = null,
            CancellationToken cancellationToken = default)
        {
            return await context.Users
                .AsNoTracking()
                .AnyAsync(u => u.Username == codigoUsuario
                            && !u.IsDeleted
                            && (excludeId == null || u.Id != excludeId),
                          cancellationToken);
        }

        public async Task<bool> ExistsByEmailAsync(
            string email,
            int? excludeId = null,
            CancellationToken cancellationToken = default)
        {
            return await context.Users
                .AsNoTracking()
                .AnyAsync(u => u.Email == email
                            && !u.IsDeleted
                            && (excludeId == null || u.Id != excludeId),
                          cancellationToken);
        }
    }
}
