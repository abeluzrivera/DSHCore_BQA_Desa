using Microsoft.AspNetCore.Http;
using SDH.Application.Ports.Services;
using System.Security.Claims;

namespace SDH.infrastructure.Persistence.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? ObtenerUsuarioActual()
        {
            // Buscamos el Email o el NameIdentifier en los Claims del usuario logueado
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email)
                ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
        }

        public string? ObtenerIpActual()
        {
            // Extrae la IP de la conexión entrante
            return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        }
    }
}
