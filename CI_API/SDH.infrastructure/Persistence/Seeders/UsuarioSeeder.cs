using Microsoft.Extensions.Logging;
using SDH.Domain.Entities.Seguridad;
using SDH.infrastructure.Persistence.Data;

namespace SDH.infrastructure.Persistence.Seeders
{
    /// <summary>
    /// Clase para inicializar datos de usuarios por defecto
    /// </summary>
    public static class UsuarioSeeder
    {
        /// <summary>
        /// Crea usuarios iniciales si no existen
        /// </summary>
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
        {
            try
            {
                logger.LogInformation("Iniciando seeding de usuarios...");

                // Verify si ya existen usuarios
                if (context.Users.Any())
                {
                    logger.LogInformation("Ya existen usuarios en la base de datos. Se omite el seeding.");
                    return;
                }

                logger.LogInformation("No se encontraron usuarios. Creando usuarios por defecto...");

                // Create usuario administrador por defecto
                // Contraseña: Admin123!
                logger.LogDebug("Generando hash para usuario Pedro Rivera administrador...");
                string adminHash = BCrypt.Net.BCrypt.HashPassword("Admin123!");

                logger.LogInformation("Creando usuario administrador con código: {CodigoUsuario}", "ADMIN001");
                Users admin = Users.Create(
                    "ADMIN001",
                    "pedro.rivera@bmachala.com",
                    adminHash,
                    "Administrador del Sistema",
                    "Administrador"
                );

                Users admin2 = Users.Create(
                   "ADMIN002",
                   "aldo.saldana@bmachala.com",
                   adminHash,
                   "Administrador del Sistema",
                   "Administrador"
               );

                // Create usuario de prueba
                // Contraseña: Usuario123!
                logger.LogDebug("Generando hash para usuario Aldo Saldaña de prueba...");
                string usuarioHash = BCrypt.Net.BCrypt.HashPassword("Usuario123!");

                logger.LogInformation("Creando usuario de prueba con código: {CodigoUsuario}", "USER001");
                Users usuario = Users.Create(
                    "USER001",
                    "usuario@bmachala.com",
                    usuarioHash,
                    "Usuario de Prueba",
                    "Usuario"
                );

                logger.LogInformation("Agregando {CantidadUsuarios} usuarios al contexto...", 2);
                context.Users.AddRange(admin, admin2, usuario);

                logger.LogDebug("Guardando cambios en la base de datos...");
                await context.SaveChangesAsync();

                logger.LogInformation("Seeding de usuarios completado exitosamente. Se crearon {CantidadUsuarios} usuarios.", 2);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al realizar el seeding de usuarios");
                throw;
            }
        }
    }
}
