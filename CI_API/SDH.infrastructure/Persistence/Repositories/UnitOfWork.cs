using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using SDH.Application.Ports.Services;
using SDH.infrastructure.Persistence.Data;

namespace SDH.infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Implementación del patrón Unit of Work para manejo de transacciones
    /// </summary>
    public class UnitOfWork(ApplicationDbContext context, ILogger<UnitOfWork> logger) : IUnitOfWork, IDisposable
    {
        private readonly ApplicationDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly ILogger<UnitOfWork> _logger = logger;
        private IDbContextTransaction? _transaction;

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Logging para diagnosticar el problema
            var entries = _context.ChangeTracker.Entries()
                .Where(e => e.State == Microsoft.EntityFrameworkCore.EntityState.Modified ||
                           e.State == Microsoft.EntityFrameworkCore.EntityState.Added ||
                           e.State == Microsoft.EntityFrameworkCore.EntityState.Deleted)
                .ToList();

            _logger.LogInformation("Cambios detectados antes de SaveChanges: {Count}", entries.Count);

            foreach (var entry in entries)
            {
                _logger.LogDebug("Entidad: {EntityType}, Estado: {State}",
                    entry.Entity.GetType().Name,
                    entry.State);
            }

            var result = await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("SaveChangesAsync ejecutado. Registros afectados: {Count}", result);

            return result;
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                if (_transaction != null)
                {
                    await _transaction.CommitAsync(cancellationToken);
                }
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
