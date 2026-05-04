using Microsoft.Extensions.Logging;
using SDH.Application.DTOs.Auth;
using SDH.Application.Ports.Services;
using SDH.Domain.Entities.Seguridad;
using SDH.Domain.Ports;
using SDH.Domain.Repositories;

namespace SDH.Application.Services
{
    public class AutenticacionService(
        IUnitOfWork unitOfWork,
        IUserRepository usuarioRepository,
        IPasswordHasher passwordHasher,
        ITokenGenerator tokenGenerator,
        ILogger<AutenticacionService> logger)
    {
        public async Task<LoginResultDto?> ValidarCredencialesAsync(string email, string clave, CancellationToken ct = default)
        {
            try
            {
                Users? usuario = await usuarioRepository.GetByEmailAsync(email, ct);

                if (usuario == null)
                {
                    logger.LogWarning("Intento de login fallido: Usuario no encontrado con email: {Email}", email);
                    return null;
                }

                // Delegar al Dominio
                usuario.Authenticate(clave, passwordHasher);
                await unitOfWork.SaveChangesAsync(ct);

                // 👈 MAGIA: Generamos los tokens e identidades aquí
                string jwtToken = tokenGenerator.GenerarJwtToken(usuario);
                var claimsPrincipal = tokenGenerator.GenerarClaimsPrincipal(usuario);

                logger.LogInformation("Usuario autenticado exitosamente: {Email}", email);

                return new LoginResultDto(usuario, jwtToken, claimsPrincipal);
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is InvalidOperationException)
            {
                logger.LogWarning("Intento de login fallido/rechazado para {Email}. Razón: {Reason}", email, ex.Message);
                return null;
            }
        }
    }
}