using System.Data.Common;
using Microsoft.Extensions.Logging;
using SDH.Application.Ports.Queries;
using SDH.Application.Services;
using SDH.Domain.Enums;
using System.Diagnostics;

namespace SDH.infrastructure.Startup
{
    /// <summary>
    /// Servicio para inicializar y precargar el caché de catálogos.
    /// Se ejecuta al inicio de la aplicación (Program.cs) para mejorar el rendimiento.
    /// </summary>
    public class CatalogosCacheInitializer(
        CatalogoService catalogoService, ICatalogQueryService catalogoQueryService,
        ILogger<CatalogosCacheInitializer> logger)
    {
        public async Task InicializarCacheAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("Iniciando precarga de caché de catálogos...");
                var sw = Stopwatch.StartNew();

                // 1. Precargar los datos CORE (Todos, Roles y Estados de Usuario)
                await catalogoService.RecargarCacheAsync(cancellationToken);

                var discrepancias = await catalogoQueryService.CheckIntegrityAsync();

                if (discrepancias.Any())
                {
                    logger.LogCritical("⚠️ ATENCIÓN: Se detectaron {Count} catálogos en BD que no están mapeados en el código C#.", discrepancias.Count);
                    // Opcional: Loggear cada uno
                    discrepancias.ForEach(d => logger.LogWarning("Inconsistencia: Grupo {G} -> Código {C}", d.GroupName, d.ValueCode));
                }

                logger.LogInformation("Caché inicializada e integridad verificada.");

                // 2. Precargar catálogos dinámicos usando tus CONSTANTES seguras
                await Task.WhenAll(
                    catalogoService.ObtenerCatalogoPorGrupoAsync(CatalogGroups.FileStatus, cancellationToken),
                    catalogoService.ObtenerCatalogoPorGrupoAsync(CatalogGroups.ContactType, cancellationToken),
                    catalogoService.ObtenerCatalogoPorGrupoAsync(CatalogGroups.ContactabilityStatus, cancellationToken)
                
                );

                sw.Stop();

                logger.LogInformation(
                    "Caché de catálogos precargado exitosamente en {Duration}ms",
                    sw.ElapsedMilliseconds);
            }
            catch (DbException ex)
            {
                logger.LogWarning(ex, "Error de base de datos al inicializar caché de catálogos. Se cargarán bajo demanda.");
            }
            catch (InvalidOperationException ex)
            {
                logger.LogWarning(ex, "Error de configuración al inicializar caché de catálogos. Se cargarán bajo demanda.");
            }
            catch (OperationCanceledException ex)
            {
                logger.LogWarning(ex, "Inicialización de caché cancelada.");
            }
        }


    }
}