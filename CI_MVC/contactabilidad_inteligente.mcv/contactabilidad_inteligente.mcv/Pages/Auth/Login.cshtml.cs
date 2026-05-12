using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SDH.Application.Services;
using System.ComponentModel.DataAnnotations;

namespace contactabilidad_inteligente.mcv.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly ILogger<LoginModel> _logger;
        private readonly AutenticacionService _autenticacionService;

        public LoginModel(ILogger<LoginModel> logger, AutenticacionService autenticacionService)
        {
            _logger = logger;
            _autenticacionService = autenticacionService;
        }

        [BindProperty]
        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "Formato de correo electrónico inválido")]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "La contraseña es requerida")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public bool RememberMe { get; set; }

        public bool ShowError { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        public IActionResult OnGet()
        {
            // Check if user is already authenticated
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToPage("/Dashboard");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            try
            {
                _logger.LogInformation($"Login attempt for user: {Username}");

                // La clave puede venir del header X-User-Password (tiene precedencia) o del campo del formulario
                string clave = Request.Headers.TryGetValue("X-User-Password", out var headerValue)
                    ? headerValue.ToString()
                    : Password;

                var authResult = await _autenticacionService.ValidarCredencialesAsync(Username, clave);

                if (authResult == null || authResult.Principal == null)
                {
                    ShowError = true;
                    ErrorMessage = authResult?.ErrorMessage ?? "Usuario o contraseña incorrectos.";
                    return Page();
                }

                AuthenticationProperties authProperties = new()
                {
                    IsPersistent = RememberMe,
                    ExpiresUtc = RememberMe ? DateTimeOffset.Now.AddDays(1) : DateTimeOffset.Now.AddHours(5)
                };


                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    authResult.Principal!,
                    authProperties);

                HttpContext.Response.Cookies.Append("AccessToken", authResult.Token!, new CookieOptions { HttpOnly = true });

                _logger.LogInformation($"User {authResult.User!.Email} logged in.");
                return RedirectToPage("/Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                ShowError = true;
                ErrorMessage = "Error al procesar la solicitud. Intente más tarde.";
                return Page();
            }
        }
    }
}
