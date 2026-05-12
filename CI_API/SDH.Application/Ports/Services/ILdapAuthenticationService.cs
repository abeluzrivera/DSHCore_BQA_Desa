using SDH.Application.DTOs.Auth;

namespace SDH.Application.Ports.Services;

public interface ILdapAuthenticationService
{
    Task<LdapAuthResult> AuthenticateAsync(
        string username,
        string password,
        CancellationToken ct = default);
}
