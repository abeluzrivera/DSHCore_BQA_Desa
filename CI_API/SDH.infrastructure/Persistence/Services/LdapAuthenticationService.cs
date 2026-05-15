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

            // Domain Scope control (OID 1.2.840.113556.1.4.1339):
            // Instructs Active Directory to suppress referrals to subordinate naming
            // contexts (e.g. DomainDnsZones, ForestDnsZones) when searching from the
            // domain root with ScopeSub. Without this, AD returns an LDAP referral for
            // each application partition it hosts, causing LdapReferralException.
            var domainScopeControl = new LdapControl(
                "1.2.840.113556.1.4.1339",
                critical: false,
                (byte[]?)null);

            var constraints = new LdapSearchConstraints
            {
                ReferralFollowing = false,
                MaxResults = 10
            };
            constraints.SetControls(domainScopeControl);

            ILdapSearchResults results = await conn.SearchAsync(
                _settings.UserSearchBase,
                LdapConnection.ScopeSub,
                filter,
                attrs,
                typesOnly: false,
                constraints,
                ct);

            LdapEntry? userEntry = null;
            var enumerator = results.ConfigureAwait(false).GetAsyncEnumerator();
            try
            {
                while (await enumerator.MoveNextAsync())
                {
                    userEntry = enumerator.Current;
                    break;
                }
            }
            catch (LdapReferralException referralEx)
            {
                // El DC devuelve un referral — significa que el UserSearchBase apunta
                // a un dominio distinto del Host configurado. Verifique Host en appsettings.
                logger.LogWarning(
                    "LDAP: Referral recibido. FailedReferral='{Failed}' Referrals=[{Referrals}]. " +
                    "Verifique que Host apunte al DC raíz del dominio '{BaseDn}'.",
                    referralEx.FailedReferral,
                    string.Join(", ", referralEx.GetReferrals() ?? []),
                    _settings.UserSearchBase);
                return Fail("No se pudo localizar al usuario. Contacte al administrador.");
            }
            finally
            {
                await enumerator.DisposeAsync();
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
            logger.LogError(ex, "LDAP: Error de conexión. ResultCode={ResultCode} Message={Message}",
                ex.ResultCode, ex.LdapErrorMessage);
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
