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
            logger.LogInformation(
                "LOGIN_INICIO usuario={Usuario} proveedor={Proveedor}",
                usernameOrEmail, useLdap ? "LDAP" : "DB");

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

            logger.LogInformation("LOGIN_LDAP_AUTH usuario={Usuario} intentando autenticacion LDAP", username);
            LdapAuthResult result = await ldapService.AuthenticateAsync(username, password, ct);

            if (!result.Success)
            {
                string reason = result.FailureReason == "no_group"
                    ? _authSettings.UnavailableMessage
                    : result.FailureReason ?? "Autenticación fallida.";
                logger.LogWarning(
                    "LOGIN_LDAP_FALLIDO usuario={Usuario} razon={Razon}",
                    username, result.FailureReason);
                return new LoginResultDto(null, null, null, reason);
            }

            logger.LogInformation(
                "LOGIN_LDAP_OK usuario={Usuario} email={Email} rol={Rol}",
                result.Username, result.Email, result.MappedRole);

            Users? usuario = await usuarioRepository.GetByUsernameAsync(result.Username!, ct);

            if (usuario == null)
            {
                usuario = Users.CreateFromLdap(
                    result.Username!, result.Email!, result.FullName!, result.MappedRole!);
                await usuarioRepository.AddUserAsync(usuario, ct);
                logger.LogInformation("LOGIN_LDAP_USUARIO_NUEVO usuario={Username}", result.Username);
            }
            else
            {
                logger.LogInformation("LOGIN_LDAP_USUARIO_EXISTENTE usuario={Username}", result.Username);
                usuario.UpdateFromLdap(result.Email!, result.FullName!, result.MappedRole!);
            }

            await unitOfWork.SaveChangesAsync(ct);

            logger.LogInformation("LOGIN_EXITOSO usuario={Usuario} metodo=LDAP", result.Username);
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
                logger.LogInformation("LOGIN_DB_BUSQUEDA usuario={Usuario}", usernameOrEmail);
                Users? usuario = await usuarioRepository.GetByEmailAsync(usernameOrEmail, ct)
                              ?? await usuarioRepository.GetByUsernameAsync(usernameOrEmail, ct);

                if (usuario == null)
                {
                    logger.LogWarning("LOGIN_DB_NO_ENCONTRADO usuario={Usuario}", usernameOrEmail);
                    return null;
                }

                logger.LogInformation("LOGIN_DB_VALIDANDO usuario={Usuario} estado={Estado}",
                    usernameOrEmail, usuario.Estado);
                usuario.Authenticate(clave, passwordHasher);
                await unitOfWork.SaveChangesAsync(ct);

                string jwtToken = tokenGenerator.GenerarJwtToken(usuario);
                var claimsPrincipal = tokenGenerator.GenerarClaimsPrincipal(usuario);

                logger.LogInformation("LOGIN_EXITOSO usuario={Usuario} metodo=DB", usernameOrEmail);
                return new LoginResultDto(usuario, jwtToken, claimsPrincipal);
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is InvalidOperationException)
            {
                logger.LogWarning("LOGIN_DB_FALLIDO usuario={Usuario} razon={Razon}",
                    usernameOrEmail, ex.Message);
                return null;
            }
        }
    }
}
