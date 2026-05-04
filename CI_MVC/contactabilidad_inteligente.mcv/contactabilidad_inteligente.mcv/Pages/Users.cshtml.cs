using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SDH.Application.Ports.Services;
using SDH.Application.Services;
using System.ComponentModel.DataAnnotations;

namespace contactabilidad_inteligente.mcv.Pages
{
    // TODO Phase 5: Full implementation migrated to UsersController (API Layer)
    [Authorize(Roles = "Administrador")]
    public class UsersModel(
        ILogger<UsersModel> logger,
        IUserQueryService usuarioQueryService,
        CatalogoService catalogoService) : PageModel
    {
        private readonly ILogger<UsersModel> _logger = logger;
        private readonly IUserQueryService _usuarioQueryService = usuarioQueryService;
        private readonly CatalogoService _catalogoService = catalogoService;

        public List<UserViewModel> Users { get; set; } = [];
        public List<SelectListItem> RolesList { get; set; } = [];
        public List<SelectListItem> EstadosList { get; set; } = [];

        [TempData] public string? SuccessMessage { get; set; }
        [TempData] public string? ErrorMessage { get; set; }

        [BindProperty]
        public CreateUserInputModel CreateUserInput { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!User.IsInRole("Administrador"))
                return RedirectToPage("/Dashboard");

            _logger.LogInformation("Pagina de usuarios accedida por: {User}", User.Identity?.Name);
            await LoadCatalogosAsync();
            await LoadUsersAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Auth/Login");
        }

        // Generate password - delegates to command service utility (no business logic in PageModel)
        public IActionResult OnPostGeneratePassword()
        {
            string password = UserCommandService.GenerateSecurePassword();
            return new JsonResult(new { success = true, password });
        }

        private async Task LoadCatalogosAsync()
        {
            var rolesDict = await _catalogoService.ObtenerRolesSistemaAsync();
            RolesList = [.. rolesDict.Select(r => new SelectListItem { Value = r.Key, Text = r.Value })];

            if (RolesList.Count == 0)
                RolesList =
                [
                    new SelectListItem { Value = "USER",  Text = "Usuario Operativo" },
                    new SelectListItem { Value = "ADMIN", Text = "Administrador" }
                ];

            var estadosDict = await _catalogoService.ObtenerEstadosUsuarioAsync();
            EstadosList = [.. estadosDict.Select(e => new SelectListItem { Value = e.Key, Text = e.Value })];
        }

        private async Task LoadUsersAsync()
        {
            var usuarios = await _usuarioQueryService.GetAllAsync();
            Users = [.. usuarios.Select(u => new UserViewModel
            {
                Id          = u.Id.ToString(),
                Name        = u.FullName,
                Email       = u.Email,
                UserCode    = u.Username,
                Role        = u.SystemRole,
                RoleBadgeClass = GetRoleBadgeClass(u.SystemRole),
                LastAccess  = u.LastLoginAt?.ToString("dd MMM yyyy, hh:mm tt") ?? "Nunca",
                Status      = u.IsActive ? "Activo" : "Inactivo",
                StatusClass = u.IsActive ? "status-active" : "status-inactive",
                IsActive    = u.IsActive,
                Initials    = GetInitials(u.FullName)
            })];
        }

        private static string GetRoleBadgeClass(string? rol)
        {
            if (string.IsNullOrWhiteSpace(rol)) return "role-viewer";
            string r = rol.ToLowerInvariant();
            if (r.Contains("administrador") || r.Contains("admin")) return "role-admin";
            if (r.Contains("supervisor") || r.Contains("super")) return "role-manager";
            if (r.Contains("auditor") || r.Contains("audit")) return "role-viewer";
            return "role-viewer";
        }

        private static string GetInitials(string nombre)
        {
            string[] parts = nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2) return $"{parts[0][0]}{parts[1][0]}".ToUpper();
            return parts.Length > 0 ? parts[0][..Math.Min(2, parts[0].Length)].ToUpper() : "??";
        }
    }

    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string RoleBadgeClass { get; set; } = string.Empty;
        public string LastAccess { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusClass { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Initials { get; set; }
    }

    public class CreateUserInputModel
    {
        [Required(ErrorMessage = "El nombre completo es requerido")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El codigo de usuario es requerido")]
        [StringLength(20)]
        public string UserCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "El rol es requerido")]
        public string Role { get; set; } = "USER";

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contrasena es requerida")]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public IFormFile? ProfileImage { get; set; }
    }
}
