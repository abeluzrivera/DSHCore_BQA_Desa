using System.Security.Claims;

namespace SDH.Application.DTOs.Auth
{
    public record LoginResultDto(
        Domain.Entities.Seguridad.Users User,
        string Token, // El JWT para las APIs
        ClaimsPrincipal Principal // La identidad para MVC/Cookies
    );
}