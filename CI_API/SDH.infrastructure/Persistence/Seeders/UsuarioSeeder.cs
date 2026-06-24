using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SDH.Domain.Entities.Seguridad;
using SDH.infrastructure.Persistence.Data;
using SDH.infrastructure.Persistence.Services;

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
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger, IConfiguration configuration)
        {
            try
            {
                logger.LogInformation("Iniciando seeding de usuarios...");

                if (context.Users.Any())
                {
                    logger.LogInformation("Ya existen usuarios en la base de datos. Se omite el seeding.");
                    return;
                }

                logger.LogInformation("No se encontraron usuarios. Creando usuarios por defecto...");

                string encryptedPassword = configuration["SeedSettings:DefaultPassword"]
                    ?? throw new InvalidOperationException("Falta configurar SeedSettings:DefaultPassword en appsettings.");

                string plainPassword = ConfigCrypto.DecryptFromEnvironment(encryptedPassword);
                string seedHash = BCrypt.Net.BCrypt.HashPassword(plainPassword);

                Users admin2 = Users.Create(
                    "ADMIN002",
                    "admin@bmachala.com",
                    seedHash,
                    "Administrador del Sistema",
                    "ADMIN"
                );

                Users usuario = Users.Create(
                    "USER001",
                    "usuario@bmachala.com",
                    seedHash,
                    "Usuario de Prueba",
                    "USER"
                );

                context.Users.AddRange(admin2, usuario);

                await context.SaveChangesAsync();

                logger.LogInformation("Seeding de usuarios completado. Se crearon 3 usuarios.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al realizar el seeding de usuarios");
                throw;
            }
        }
    }
}