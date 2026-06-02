using Microsoft.EntityFrameworkCore;
using SDH.Domain.Entities.Operative;
using SDH.Domain.Repositories;
using SDH.infrastructure.Persistence.Data;

namespace SDH.infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Implementacion del repositorio de Cliente
    /// </summary>
    public class CustomerRepository(ApplicationDbContext context) : ICustomerRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<Client?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            return await _context.Client
                .Include(c => c.Contacts)
                .Include(c => c.Addresses)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<Client?> GetByIdentificationAsync(string identificacion, CancellationToken cancellationToken = default)
        {
            return await _context.Client
                .Include(c => c.Contacts)
                .Include(c => c.Addresses)
                .FirstOrDefaultAsync(c => c.Identification == identificacion, cancellationToken);
        }

        public async Task<Client> AddCustomerAsync(Client cliente, CancellationToken cancellationToken = default)
        {
            await _context.Client.AddAsync(cliente, cancellationToken);
            return cliente;
        }

        public Task UpdateCustomerAsync(Client cliente, CancellationToken cancellationToken = default)
        {
            _context.Client.Update(cliente);
            return Task.CompletedTask;
        }

        public async Task<bool> ExistsByIdentificationAsync(string identificacion, CancellationToken cancellationToken = default)
        {
            return await _context.Client
                .AnyAsync(c => c.Identification == identificacion, cancellationToken);
        }


    }
}
