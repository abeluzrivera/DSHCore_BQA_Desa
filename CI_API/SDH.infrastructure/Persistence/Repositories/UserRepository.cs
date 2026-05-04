using Microsoft.EntityFrameworkCore;
using SDH.Domain.Entities.Seguridad;
using SDH.Domain.Repositories;
using SDH.infrastructure.Persistence.Data;

namespace SDH.infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Implementaci�n del repositorio de Usuario
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Users?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public async Task<Users?> GetByUsernameAsync(string codigoUsuario, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == codigoUsuario, cancellationToken);
        }

        public async Task<Users?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<IEnumerable<Users>> GetActiveAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Where(u => u.IsActive && !u.IsDeleted)
                .OrderBy(u => u.FullName)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Users>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Where(u => !u.IsDeleted)
                .OrderBy(u => u.FullName)
                .ToListAsync(cancellationToken);
        }

        public async Task<Users> AddUserAsync(Users usuario, CancellationToken cancellationToken = default)
        {
            await _context.Users.AddAsync(usuario, cancellationToken);
            return usuario;
        }

        public Task UpdateUserAsync(Users usuario, CancellationToken cancellationToken = default)
        {
            _context.Users.Update(usuario);
            return Task.CompletedTask;
        }

        public async Task<bool> ExistsByUsernameAsync(string codigoUsuario, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AnyAsync(u => u.Username == codigoUsuario, cancellationToken);
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email, cancellationToken);
        }
    }
}
