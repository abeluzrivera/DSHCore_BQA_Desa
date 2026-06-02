using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SDH.Application.DTOs.Auth;
using SDH.Application.Ports.Services;
using SDH.Application.Settings;
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
        IOptions<AuthSettings> authOptions,
        ILogger<AutenticacionService> logger,
        ILdapAuthenticationService? ldapService = null)
    {
        private readonly AuthSettings _authSettings = authOptions.Value;

        public async Task<LoginResultDto?> ValidarCredencialesAsync(
            string usernameOrEmail, string clave, CancellationToken ct = default)
        {
            bool useLdap = _authSettings.Provider.Equals("LDAP", StringComparison.OrdinalIgnoreCase);
            return useLdap
                ? await ValidarPorLdapAsync(usernameOrEmail, clave, ct)
                : await ValidarPorBaseDatosAsync(usernameOrEmail, clave, ct);
        }

        private async Task<LoginResultDto?> ValidarPorLdapAsync(
            string username, string password, CancellationToken ct)
        {
            if (ldapService == null)
                throw new InvalidOperationException(
                    "AuthSettings:Provider = LDAP pero ILdapAuthenticationService no está registrado.");

            LdapAuthResult result = await ldapService.AuthenticateAsync(username, password, ct);

            if (!result.Success)
            {
                string reason = result.FailureReason == "no_group"
                    ? _authSettings.UnavailableMessage
                    : result.FailureReason ?? "Autenticación fallida.";
                return new LoginResultDto(null, null, null, reason);
            }

            // STATELESS — objeto transitorio en memoria, sin escritura en BD
            var usuario = Users.CreateTransient(
                result.Username!, result.Email!, result.FullName!, result.MappedRole!);

            logger.LogInformation("LDAP OK (stateless): '{Username}' → rol '{Role}'",
                result.Username, result.MappedRole);

            return new LoginResultDto(
                usuario,
                tokenGenerator.GenerarJwtToken(usuario),
                tokenGenerator.GenerarClaimsPrincipal(usuario));
        }

        private async Task<LoginResultDto?> ValidarPorBaseDatosAsync(
            string usernameOrEmail, string clave, CancellationToken ct)
        {
            try
            {
                Users? usuario = await usuarioRepository.GetByEmailAsync(usernameOrEmail, ct)
                              ?? await usuarioRepository.GetByUsernameAsync(usernameOrEmail, ct);

                if (usuario == null)
                {
                    logger.LogWarning("Intento de login fallido: Usuario no encontrado: {UsernameOrEmail}", usernameOrEmail);
                    return null;
                }

                usuario.Authenticate(clave, passwordHasher);
                await unitOfWork.SaveChangesAsync(ct);

                string jwtToken = tokenGenerator.GenerarJwtToken(usuario);
                var claimsPrincipal = tokenGenerator.GenerarClaimsPrincipal(usuario);

                logger.LogInformation("Usuario autenticado exitosamente: {UsernameOrEmail}", usernameOrEmail);

                return new LoginResultDto(usuario, jwtToken, claimsPrincipal);
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is InvalidOperationException)
            {
                logger.LogWarning("Intento de login fallido/rechazado para {UsernameOrEmail}. Razón: {Reason}", usernameOrEmail, ex.Message);
                return null;
            }
        }
    }
}
