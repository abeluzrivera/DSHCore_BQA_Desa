using Microsoft.Extensions.Logging;
using SDH.Application.Common; // Donde tienes tu clase Result y Result<T>
using SDH.Application.Ports.Services;
using SDH.Domain.Entities.Parametro;
using SDH.Domain.Repositories;

namespace SDH.Application.Services
{
    /// <summary>
    /// COMMAND SERVICE (Casos de Uso de Escritura).
    /// Se encarga EXCLUSIVAMENTE de crear, actualizar o eliminar catálogos.
    /// Trabaja con el Agregado Raíz (GrupoCatalogo) y el UnitOfWork.
    /// </summary>
    public class CatalogoCommandService(
        IUnitOfWork unitOfWork,
        ICatalogRepository repository, // Lado de escritura del repo
        CatalogoService queryService,   // Lo inyectamos SÓLO para limpiar la caché
        ILogger<CatalogoCommandService> logger)
    {
        /// <summary>
        /// CASO DE USO 1: Create un nuevo grupo de catálogo (Ej: TIPO_SANGRE)
        /// </summary>
        public async Task<Result<int>> CrearGrupoAsync(
            string nombreGrupo, string? descripcion, bool esSistema, string usuarioCreacion, CancellationToken cancellationToken = default)
        {
            try
            {
                // 1. Validar regla de negocio: ¿Ya existe?
                nombreGrupo = nombreGrupo.Trim().ToUpper();
                if (await repository.ExistsGroupByNameAsync(nombreGrupo, cancellationToken))
                {
                    return Result<int>.Failure($"El grupo de catálogo '{nombreGrupo}' ya existe.");
                }

                // 2. Create usando el Factory del Dominio
                CatalogGroup nuevoGrupo = CatalogGroup.Create(nombreGrupo, descripcion, esSistema, usuarioCreacion);

                // 3. Persistir
                await repository.AddGroupAsync(nuevoGrupo, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                // 4. Limpiar caché general porque hay un grupo nuevo
                queryService.LimpiarCacheGeneral();

                logger.LogInformation("Grupo de catálogo creado: {NombreGrupo} con ID {Id}", nombreGrupo, nuevoGrupo.Id);

                return Result<int>.Success(nuevoGrupo.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al crear el grupo de catálogo {NombreGrupo}", nombreGrupo);
                return Result<int>.Failure("Ocurrió un error interno al crear el catálogo.");
            }
        }

        /// <summary>
        /// CASO DE USO 3: Activar o Inactivar un ítem específico
        /// </summary>
        public async Task<Result> CambiarEstadoItemAsync(
            string nombreGrupo, string codigoValor, bool nuevoEstado, string usuarioModificacion, CancellationToken cancellationToken = default)
        {
            try
            {
                nombreGrupo = nombreGrupo.Trim().ToUpper();

                // 1. Cargar el Agregado
                CatalogGroup? grupo = await repository.GetGroupByNameAsync(nombreGrupo, cancellationToken);

                if (grupo == null) return Result.Failure($"No se encontró el grupo '{nombreGrupo}'.");

                // 2. Buscar el ítem en la memoria del Agregado
                CatalogItem? item = grupo.Items.FirstOrDefault(i => i.ValueCode == codigoValor);

                if (item == null) return Result.Failure($"El ítem '{codigoValor}' no existe en el grupo '{nombreGrupo}'.");

                // 3. Ejecutar comportamiento del dominio
                item.ChangeStatus(nuevoEstado, usuarioModificacion);

                // 4. Guardar y limpiar caché
                await unitOfWork.SaveChangesAsync(cancellationToken);
                queryService.LimpiarCacheGrupo(nombreGrupo);

                logger.LogInformation("Estado del ítem {Codigo} ({Grupo}) cambiado a {Estado}", codigoValor, nombreGrupo, nuevoEstado);

                return Result.Success();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al cambiar estado del ítem {Codigo} en {Grupo}", codigoValor, nombreGrupo);
                return Result.Failure("Ocurrió un error al actualizar el estado del ítem.");
            }
        }
    }
}