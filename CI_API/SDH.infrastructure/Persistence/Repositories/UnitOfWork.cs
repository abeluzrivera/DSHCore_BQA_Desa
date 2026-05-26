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
        private bool _disposed = false;

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
            if (_disposed)
                throw new ObjectDisposedException(nameof(UnitOfWork));

            if (_transaction != null)
                throw new InvalidOperationException("A transaction is already in progress.");

            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UnitOfWork));

            if (_transaction == null)
            {
                _logger.LogWarning("CommitTransactionAsync called without an active transaction.");
                return;
            }

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                await _transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during CommitTransactionAsync, attempting rollback.");
                try
                {
                    if (_transaction != null)
                    {
                        await _transaction.RollbackAsync(cancellationToken);
                    }
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "Rollback failed after commit error.");
                }

                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    try
                    {
                        await _transaction.DisposeAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to dispose transaction in CommitTransactionAsync finalizer.");
                    }
                    finally
                    {
                        _transaction = null;
                    }
                }
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_disposed)
            {
                _logger.LogWarning("Rollback called after UnitOfWork disposed.");
                return;
            }

            if (_transaction == null)
            {
                _logger.LogDebug("RollbackTransactionAsync called with no active transaction.");
                return;
            }

            try
            {
                await _transaction.RollbackAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while rolling back transaction.");
            }
            finally
            {
                try
                {
                    await _transaction.DisposeAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to dispose transaction during rollback.");
                }
                finally
                {
                    _transaction = null;
                }
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            // Dispose synchronous resources
            try
            {
                _transaction?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error disposing transaction in Dispose().");
            }

            try
            {
                _context.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error disposing context in Dispose().");
            }

            _disposed = true;
        }
    }
}
