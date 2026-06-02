using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SDH.Application.Ports.Services;
using SDH.Domain.Entities.Seguridad;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SDH.infrastructure.Persistence.Services
{
    public class TokenGenerator(IConfiguration configuration, IConfigDecryptionService decryptor) : ITokenGenerator
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly IConfigDecryptionService _decryptor = decryptor;

        // 1. Genera la Identidad para las Cookies (MVC / Razor Pages)
        public ClaimsPrincipal GenerarClaimsPrincipal(Users usuario)
        {
            var claims = ConstruirClaimsBasicos(usuario);
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            return new ClaimsPrincipal(claimsIdentity);
        }

        // 2. Genera el string JWT para tu futura API Móvil/Frontend
        public string GenerarJwtToken(Users usuario)
        {
            var claims = ConstruirClaimsBasicos(usuario);

            // Tienes que tener esto en tu appsettings.json
            var secretKey = _configuration["JwtSettings:SecretKey"]
                ?? throw new InvalidOperationException("Falta configurar JwtSettings:SecretKey");
            secretKey = _decryptor.Decrypt(secretKey);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(5), // Tiempo de expiración del JWT
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Método privado para NO duplicar la lista de Claims entre MVC y JWT
        private static List<Claim> ConstruirClaimsBasicos(Users usuario)
        {
            // Para usuarios LDAP (CreateTransient) el Id es 0 — usamos Username como identificador único
            string identifier = usuario.Id > 0
                ? usuario.Id.ToString()
                : usuario.Username;

            return
            [
                new(ClaimTypes.NameIdentifier, identifier),
                new(ClaimTypes.Name, usuario.FullName),
                new(ClaimTypes.Email, usuario.Email),
                new(ClaimTypes.Role, usuario.SystemRole),
                new("Username", usuario.Username),
                new("DisplayName", usuario.FullName),
                new(ClaimTypes.Sid, identifier)
            ];
        }
    }
}