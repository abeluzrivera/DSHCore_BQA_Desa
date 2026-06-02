using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SDH.Application.DTOs.Users;
using SDH.Application.Ports.Services;
using SDH.Application.Services;
using System.Security.Claims;
using contactabilidad_inteligente.mcv.Filters;

namespace contactabilidad_inteligente.mcv.Controllers
{
    /// <summary>
    /// API REST para gestión de usuarios del sistema.
    /// Sigue el contrato DS-Back: retorna DTOs puros, sin ViewModels ni clases CSS.
    /// El cliente (JS) es responsable de aplicar presentación visual.
    /// </summary>
    [Authorize(Roles = "ADMIN")]
    [ApiController]
    [Route("api/users")]
    [ApiExceptionFilter]
    public class UsersController(
        ILogger<UsersController> logger,
        UserCommandService commandService,
        IUserQueryService queryService,
        CatalogoService catalogoService) : ControllerBase
    {
        // ─── READ ──────────────────────────────────────────────────────────────────

        /// <summary>
        /// GET /api/users
        /// Lista todos los usuarios no eliminados ordenados por nombre.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserListItemDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        {
            var usuarios = await queryService.GetAllAsync(cancellationToken);
            return Ok(new { success = true, data = usuarios });
        }

        /// <summary>
        /// GET /api/users/{id}
        /// Retorna detalle para el modal de edición.
        /// Incluye rolCodigo (código catálogo) además del RolSistema (texto visual).
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
        {
            var usuario = await queryService.GetByIdAsync(id, cancellationToken);

            if (usuario is null)
                return NotFound(new { success = false, message = "Usuario no encontrado" });

            var rolesDict = await catalogoService.ObtenerRolesSistemaAsync(cancellationToken);
            string rolCodigo = rolesDict
                .FirstOrDefault(r => string.Equals(r.Value, usuario.SystemRole, StringComparison.OrdinalIgnoreCase))
                .Key ?? string.Empty;

            return Ok(new
            {
                success = true,
                data = new
                {
                    usuario.Id,
                    usuario.FullName,
                    usuario.Username,
                    usuario.Email,
                    usuario.SystemRole,
                    rolCodigo,
                    usuario.IsActive
                }
            });
        }

        // ─── WRITE ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// POST /api/users
        /// Crea un nuevo usuario. Valida unicidad de código y email en el command service.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Create(
            [FromBody] CreateUsuarioRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                });

            string usuarioActual = ObtenerUsuarioActual();

            var command = new CreateUsuarioCommand(
                request.FullName,
                request.Username,
                request.Email,
                request.RoleCode,
                request.PlainPassword,
                request.IsActive,
                usuarioActual);

            var result = await commandService.CreateAsync(command, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new { success = false, message = result.ErrorMessage });

            return Created($"/api/users/{result.Value}",
                new { success = true, message = $"Usuario {request.FullName} creado exitosamente.", id = result.Value });
        }

        /// <summary>
        /// PUT /api/users/{id}
        /// Actualiza datos del usuario. No modifica contraseña.
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateUsuarioRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                });

            var command = new UpdateUsuarioCommand(
                id,
                request.FullName,
                request.Username,
                request.Email,
                request.RoleCode,
                request.IsActive,
                ObtenerUsuarioActual());

            var result = await commandService.UpdateAsync(command, cancellationToken);

            if (!result.IsSuccess)
                return result.ErrorMessage.Contains("no encontrado", StringComparison.OrdinalIgnoreCase)
                    ? NotFound(new { success = false, message = result.ErrorMessage })
                    : BadRequest(new { success = false, message = result.ErrorMessage });

            return Ok(new { success = true, message = $"Usuario {request.FullName} actualizado exitosamente." });
        }

        /// <summary>
        /// PATCH /api/users/{id}/toggle-status
        /// Invierte el estado activo/inactivo. Retorna el nuevo estado para actualizar la UI sin recargar.
        /// </summary>
        [HttpPatch("{id:int}/toggle-status")]
        [ProducesResponseType(typeof(UserStatusToggleDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ToggleStatus(int id, CancellationToken cancellationToken = default)
        {
            var result = await commandService.ToggleStatusAsync(id, cancellationToken);

            if (!result.IsSuccess)
                return result.ErrorMessage.Contains("no encontrado", StringComparison.OrdinalIgnoreCase)
                    ? NotFound(new { success = false, message = result.ErrorMessage })
                    : BadRequest(new { success = false, message = result.ErrorMessage });

            return Ok(new
            {
                success = true,
                message = $"Usuario {(result.Value!.NewStatus ? "activado" : "desactivado")} exitosamente.",
                data = result.Value
            });
        }

        /// <summary>
        /// POST /api/users/generate-password
        /// Genera una contraseña segura aleatoria. No persiste nada.
        /// </summary>
        [HttpPost("generate-password")]
        [ProducesResponseType(200)]
        public IActionResult GeneratePassword()
        {
            return Ok(new { success = true, password = UserCommandService.GenerateSecurePassword() });
        }

        // ─── Helpers ───────────────────────────────────────────────────────────────

        private string ObtenerUsuarioActual() =>
            User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? "SISTEMA";
    }

    // ─── Request DTOs (solo API Layer — sin DataAnnotations, ApiController valida automáticamente) ───

    public record CreateUsuarioRequest(
        string FullName,
        string Username,
        string Email,
        string RoleCode,
        string PlainPassword,
        bool IsActive);

    public record UpdateUsuarioRequest(
        string FullName,
        string Username,
        string Email,
        string RoleCode,
        bool IsActive);
}
