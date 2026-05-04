using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using SDH.Application.Services;

namespace contactabilidad_inteligente.mcv.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(AutenticacionService autenticacionService) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await autenticacionService.ValidarCredencialesAsync(request.Email, request.Password);

            if (result == null) return Unauthorized();

            return Ok(new { Token = result.Token });
        }
    }
}
