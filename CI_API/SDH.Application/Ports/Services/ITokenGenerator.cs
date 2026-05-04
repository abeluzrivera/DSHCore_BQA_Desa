using SDH.Domain.Entities.Seguridad;
using System.Security.Claims;

namespace SDH.Application.Ports.Services
{
    public interface ITokenGenerator
    {
        string GenerarJwtToken(Users usuario);
        ClaimsPrincipal GenerarClaimsPrincipal(Users usuario);
    }
}