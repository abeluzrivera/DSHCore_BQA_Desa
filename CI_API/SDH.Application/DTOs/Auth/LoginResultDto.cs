using System.Security.Claims;

namespace SDH.Application.DTOs.Auth
{
    public record LoginResultDto
    {
        public Domain.Entities.Seguridad.Users? User { get; init; }
        public string? Token { get; init; }
        public ClaimsPrincipal? Principal { get; init; }
        public string? ErrorMessage { get; init; }

        // Constructor éxito (compatibilidad existente)
        public LoginResultDto(Domain.Entities.Seguridad.Users user, string token, ClaimsPrincipal principal)
        {
            User = user;
            Token = token;
            Principal = principal;
        }

        // Constructor error con mensaje de negocio
        public LoginResultDto(Domain.Entities.Seguridad.Users? user, string? token, ClaimsPrincipal? principal, string errorMessage)
        {
            User = user;
            Token = token;
            Principal = principal;
            ErrorMessage = errorMessage;
        }
    }
}
