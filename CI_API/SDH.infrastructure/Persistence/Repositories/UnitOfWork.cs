using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using SDH.Application.Ports.Services;
using SDH.infrastructure.Persistence.Data;

namespace SDH.infrastructure.Persistence.Repositories
{
    public class UnitOfWork(ApplicationDbContext context, ILogger<UnitOfWork> logger) : IUnitOfWork, IDisposable
    {
        private readonly ApplicationDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly ILogger<UnitOfWork> _logger = logger;
        private IDbContextTransaction? _transaction;
        private bool _disposed = false;
        private bool _isBeginningTransaction = false;

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = _context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified ||
                           e.State == EntityState.Added ||
                           e.State == EntityState.Deleted)
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

            if (_isBeginningTransaction)
                throw new InvalidOperationException("BeginTransactionAsync is already in progress.");

            _isBeginningTransaction = true;
            try
            {
                _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            }
            finally
            {
                _isBeginningTransaction = false;
            }
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
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update error during CommitTransactionAsync, attempting rollback.");
                await TryRollbackAsync(cancellationToken);
                throw;
            }
            catch (DbException ex)
            {
                _logger.LogError(ex, "Database error during CommitTransactionAsync, attempting rollback.");
                await TryRollbackAsync(cancellationToken);
                throw;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("CommitTransactionAsync was cancelled, attempting rollback.");
                await TryRollbackAsync(cancellationToken);
                throw;
            }
            finally
            {
                await TryDisposeTransactionAsync();
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
            catch (DbException ex)
            {
                _logger.LogError(ex, "Error while rolling back transaction.");
            }
            finally
            {
                await TryDisposeTransactionAsync();
            }
        }

        private async Task TryRollbackAsync(CancellationToken cancellationToken)
        {
            if (_transaction == null) return;
            try
            {
                await _transaction.RollbackAsync(cancellationToken);
            }
            catch (DbException rollbackEx)
            {
                _logger.LogError(rollbackEx, "Rollback failed after commit error.");
            }
        }

        private async Task TryDisposeTransactionAsync()
        {
            if (_transaction == null) return;
            try
            {
                await _transaction.DisposeAsync();
            }
            catch (DbException ex)
            {
                _logger.LogWarning(ex, "Failed to dispose transaction.");
            }
            finally
            {
                _transaction = null;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _transaction?.Dispose();
            _transaction = null;
            _context.Dispose();
            _disposed = true;
        }
    }
}
