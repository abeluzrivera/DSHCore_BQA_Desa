using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SDH.Application.DTOs;
using SDH.Application.Ports.Queries;
using SDH.Domain.Enums;

namespace SDH.Application.Services
{
    /// <summary>
    /// Servicio de aplicación para lectura y gestión rápida de catálogos parametrizables.
    /// Arquitectura CQRS: Utiliza ICatalogoQueryService (Read Model) y MemoryCache.
    /// </summary>
    public class CatalogoService(
        ICatalogQueryService queryService,
        ILogger<CatalogoService> logger,
        IMemoryCache cache)
    {
        private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(1);

        // Claves de caché
        private const string CACHE_KEY_ALL_CATALOGOS = "AllCatalogos_Vista";
        private const string CACHE_KEY_PREFIX_GRUPO = "Catalogo_Grupo_";

        // ────────────────────────────────────────────────────────────
        // MÉTODOS DE LECTURA (Optimizados con Caché y Vista)
        // ────────────────────────────────────────────────────────────

        public async Task<IReadOnlyList<CatalogDetail>> ObtenerTodosCatalogosAsync(CancellationToken cancellationToken = default)
        {
            return await cache.GetOrCreateAsync(CACHE_KEY_ALL_CATALOGOS, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _cacheDuration;
                logger.LogDebug("Cache Miss: Cargando TODOS los catálogos desde la BD.");

                return await queryService.GetActiveAsync(cancellationToken);
            }) ?? [];
        }

        public async Task<IReadOnlyList<CatalogDetail>> ObtenerCatalogoPorGrupoAsync(string nombreGrupo, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(nombreGrupo)) return [];

            // Estandarizamos a mayúsculas para evitar errores de tipeo al buscar en caché
            string cacheKey = $"{CACHE_KEY_PREFIX_GRUPO}{nombreGrupo.ToUpper()}";

            return await cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _cacheDuration;
                entry.Size = 1;
                logger.LogDebug("Cache Miss: Cargando catálogo {NombreGrupo} desde la BD.", nombreGrupo);

                var items = await queryService.GetByGroupNameAsync(nombreGrupo, cancellationToken);

                if (items.Count == 0)
                {
                    logger.LogWarning("No se encontraron ítems para el catálogo {NombreGrupo}", nombreGrupo);
                }

                return items;
            }) ?? [];
        }

        public async Task<CatalogDetail?> ObtenerItemPorCodigoAsync(string nombreGrupo, string codigoValor, CancellationToken cancellationToken = default)
        {
            // Magia pura: Traemos el grupo de la RAM (Caché) y filtramos sin tocar la BD
            var grupo = await ObtenerCatalogoPorGrupoAsync(nombreGrupo, cancellationToken);

            return grupo.FirstOrDefault(i => i.ValueCode.Equals(codigoValor, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<string> ObtenerTextoVisualAsync(string nombreGrupo, string codigoValor, string valorPorDefecto = "", CancellationToken cancellationToken = default)
        {
            var item = await ObtenerItemPorCodigoAsync(nombreGrupo, codigoValor, cancellationToken);
            return item?.DisplayName ?? valorPorDefecto;
        }

        public async Task<bool> ValidarCodigoExisteAsync(string nombreGrupo, string codigoValor, CancellationToken cancellationToken = default)
        {
            var item = await ObtenerItemPorCodigoAsync(nombreGrupo, codigoValor, cancellationToken);
            return item != null && item.IsActive;
        }

        // ────────────────────────────────────────────────────────────
        // MÉTODOS ESPECÍFICOS PARA EL CORE DEL NEGOCIO
        // ────────────────────────────────────────────────────────────

        public async Task<Dictionary<string, string>> ObtenerRolesSistemaAsync(CancellationToken cancellationToken = default)
        {
            var items = await ObtenerCatalogoPorGrupoAsync(CatalogGroups.SystemRole, cancellationToken);
            return items.ToDictionary(i => i.ValueCode, i => i.DisplayName);
        }

        public async Task<Dictionary<string, string>> ObtenerEstadosUsuarioAsync(CancellationToken cancellationToken = default)
        {
            var items = await ObtenerCatalogoPorGrupoAsync(CatalogGroups.UserStatus, cancellationToken);
            return items.ToDictionary(i => i.ValueCode, i => i.DisplayName);
        }

        // ────────────────────────────────────────────────────────────
        // MANEJO DE CACHÉ
        // ────────────────────────────────────────────────────────────

        public void LimpiarCacheGeneral()
        {
            cache.Remove(CACHE_KEY_ALL_CATALOGOS);
            cache.Remove($"{CACHE_KEY_PREFIX_GRUPO}{CatalogGroups.SystemRole}");
            cache.Remove($"{CACHE_KEY_PREFIX_GRUPO}{CatalogGroups.UserStatus}");

            logger.LogInformation("Caché de catálogos limpiado completamente");
        }

        public void LimpiarCacheGrupo(string nombreGrupo)
        {
            // Borramos la lista "All" para que no quede desactualizada
            cache.Remove(CACHE_KEY_ALL_CATALOGOS);
            // Borramos el grupo específico
            cache.Remove($"{CACHE_KEY_PREFIX_GRUPO}{nombreGrupo.ToUpper()}");

            logger.LogInformation("Caché del catálogo {NombreGrupo} limpiado", nombreGrupo);
        }

        public async Task RecargarCacheAsync(CancellationToken cancellationToken = default)
        {
            LimpiarCacheGeneral();

            // Precargar datos vitales al iniciar
            await ObtenerTodosCatalogosAsync(cancellationToken);
            await ObtenerRolesSistemaAsync(cancellationToken);
            await ObtenerEstadosUsuarioAsync(cancellationToken);

            logger.LogInformation("Caché de catálogos recargado exitosamente");
        }
    }
}
