using Microsoft.EntityFrameworkCore;
using SDH.Domain.Entities.Parametro;
using SDH.Domain.Repositories;
using SDH.infrastructure.Persistence.Data;

namespace SDH.Infrastructure.Persistence.Repositories
{
    // Usamos Primary Constructors de C# 12
    public class CatalogRepository(ApplicationDbContext context) : ICatalogRepository
    {
        public async Task<CatalogGroup?> GetGroupByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            // LADO ESCRITURA: Hacemos el Include de los Items porque si cargamos 
            // este grupo es probablemente para modificarlo o agregarle nuevos ítems.
            return await context.CatalogGroup
                .Include(g => g.Items)
                .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
        }

        public async Task<CatalogGroup?> GetGroupByNameAsync(string nombreGrupo, CancellationToken cancellationToken = default)
        {
            return await context.CatalogGroup
                .Include(g => g.Items)
                .FirstOrDefaultAsync(g => g.GroupName == nombreGrupo, cancellationToken);
        }

        public async Task<bool> ExistsGroupByNameAsync(string nombreGrupo, CancellationToken cancellationToken = default)
        {
            // AnyAsync es lo más rápido para validaciones: "SELECT TOP 1 1 FROM..."
            return await context.CatalogGroup
                .AnyAsync(g => g.GroupName == nombreGrupo, cancellationToken);
        }

        public async Task AddGroupAsync(CatalogGroup grupo, CancellationToken cancellationToken = default)
        {
            await context.CatalogGroup.AddAsync(grupo, cancellationToken);
            // Recuerda: No hacemos SaveChangesAsync aquí. De eso se encarga el UnitOfWork.
        }

        public Task UpdateGroupAsync(CatalogGroup grupo, CancellationToken cancellationToken = default)
        {
            context.CatalogGroup.Update(grupo);
            return Task.CompletedTask;
        }
    }
}