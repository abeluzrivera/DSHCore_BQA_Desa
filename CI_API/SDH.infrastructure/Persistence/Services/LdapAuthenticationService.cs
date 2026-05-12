using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Novell.Directory.Ldap;
using SDH.Application.DTOs.Auth;
using SDH.Application.Ports.Services;
using SDH.Application.Settings;

namespace SDH.infrastructure.Persistence.Services;

public class LdapAuthenticationService(
    IOptions<LdapSettings> options,
    IConfigDecryptionService decryptor,
    ILogger<LdapAuthenticationService> logger) : ILdapAuthenticationService
{
    private readonly LdapSettings _settings = options.Value;

    public async Task<LdapAuthResult> AuthenticateAsync(
        string username, string password, CancellationToken ct = default)
    {
        try
        {
            string bindPassword = decryptor.Decrypt(_settings.BindPassword);

            var connOptions = _settings.UseSSL
                ? new LdapConnectionOptions().UseSsl()
                : new LdapConnectionOptions();

            using var conn = new LdapConnection(connOptions);
            await conn.ConnectAsync(_settings.Host, _settings.Port, ct);
            await conn.BindAsync(_settings.BindDn, bindPassword, ct);

            string filter = string.Format(_settings.UserSearchFilter, LdapEscape(username));
            string[] attrs = ["dn", "cn", "mail", "displayName", _settings.GroupAttribute];

            ILdapSearchResults results = await conn.SearchAsync(
                _settings.UserSearchBase,
                LdapConnection.ScopeSub,
                filter,
                attrs,
                typesOnly: false,
                ct);

            LdapEntry? userEntry = null;
            await foreach (LdapEntry entry in results.ConfigureAwait(false))
            {
                userEntry = entry;
                break;
            }

            if (userEntry == null)
                return Fail("Usuario no encontrado en el directorio activo.");

            // Bind con credenciales del usuario para validar contraseña
            try { await conn.BindAsync(userEntry.Dn, password, ct); }
            catch (LdapException) { return Fail("Credenciales inválidas."); }

            // Mapear grupo AD al rol del sistema
            string? mappedRole = null;
            LdapAttribute? groupAttr = userEntry.GetOrDefault(_settings.GroupAttribute);
            if (groupAttr != null)
            {
                foreach (string groupDn in groupAttr.StringValueArray)
                {
                    string cn = groupDn.Split(',')[0]
                        .Replace("CN=", "", StringComparison.OrdinalIgnoreCase).Trim();
                    mappedRole = _settings.RoleGroupMappings
                        .FirstOrDefault(k => k.Value.Equals(cn, StringComparison.OrdinalIgnoreCase)).Key;
                    if (mappedRole != null) break;
                }
            }

            if (mappedRole == null)
            {
                logger.LogWarning("LDAP: '{Username}' autenticado pero sin grupo habilitado.", username);
                return Fail("no_group");
            }

            string displayName = userEntry.GetOrDefault("displayName")?.StringValue
                                 ?? userEntry.GetOrDefault("cn")?.StringValue
                                 ?? username;
            string email = userEntry.GetOrDefault("mail")?.StringValue
                           ?? $"{username}@bmachala.com";

            logger.LogInformation("LDAP OK: '{Username}' → rol '{Role}'", username, mappedRole);
            return new LdapAuthResult(true, mappedRole, displayName, email, username, null);
        }
        catch (LdapException ex) when (ex.ResultCode != LdapException.InvalidCredentials)
        {
            logger.LogError(ex, "LDAP: Error de conexión.");
            return Fail("Error al conectar con el directorio activo. Intente más tarde.");
        }
    }

    private static LdapAuthResult Fail(string reason) =>
        new(false, null, null, null, null, reason);

    // OWASP RFC 4515 — previene LDAP Injection
    private static string LdapEscape(string input) =>
        input.Replace("\\", "\\5C").Replace("*", "\\2A")
             .Replace("(", "\\28").Replace(")", "\\29").Replace("\0", "\\00");
}
