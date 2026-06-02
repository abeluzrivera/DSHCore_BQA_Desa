using SDH.Domain.Entities.Parametro;

namespace SDH.Domain.Repositories
{
    /// <summary>
    /// LADO COMANDOS (Escritura): Port para el repositorio de Catálogos.
    /// Solo maneja Entidades de Dominio puras.
    /// </summary>
    public interface ICatalogRepository
    {
        // Consultas necesarias para validar reglas de negocio antes de escribir
        Task<CatalogGroup?> GetGroupByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<CatalogGroup?> GetGroupByNameAsync(string nombreGrupo, CancellationToken cancellationToken = default);
        Task<bool> ExistsGroupByNameAsync(string nombreGrupo, CancellationToken cancellationToken = default);

        // Operaciones de persistencia del Agregado
        Task AddGroupAsync(CatalogGroup grupo, CancellationToken cancellationToken = default);
        Task UpdateGroupAsync(CatalogGroup grupo, CancellationToken cancellationToken = default);
    }
}