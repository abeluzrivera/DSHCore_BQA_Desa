using contactabilidad_inteligente.mcv.Mappers;
using contactabilidad_inteligente.mcv.Models.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SDH.Application.DTOs.Clients;
using SDH.Application.Ports.Services;
using SDH.Domain.Enums;
using SDH.Domain.Extensions;
using System.Text.Json;

namespace contactabilidad_inteligente.mcv.Pages
{
    [Authorize]
    public class DashboardModel(
        ILogger<DashboardModel> logger,
        ICustomerCacheService cacheService,
        IClienteQueryService queryService,
        IFileExtractionService fileExtractionService,
        IWebHostEnvironment env) : PageModel
    {
        private readonly ILogger<DashboardModel> _logger = logger;
        private readonly ICustomerCacheService _cacheService = cacheService;
        private readonly IClienteQueryService _queryService = queryService;
        private readonly IFileExtractionService _fileExtractionService = fileExtractionService;
        private readonly IWebHostEnvironment _env = env;

        public List<ClientViewModel> Clients { get; set; } = [];

        public int TotalTodos => Clients.Count;
        public int TotalPorVerificar => Clients.Count(c => c.Status != "Verificado");
        public int TotalVerificados => Clients.Count(c => c.Status == "Verificado");

        public string? LastUploadedFile { get; set; }

        [BindProperty(SupportsGet = true, Name = "ids")]
        public string? FilterIds { get; set; }

        [TempData]
        public string? LoteIds { get; set; }

        /// <summary>
        /// Mapeo de tipo de contacto → grupo semántico. 
        /// Emitido al JS como window.CONTACT_TYPE_TO_GROUP para que la lógica de agrupación
        /// en el cliente sea consistente con la regla de dominio (Cliente.GetGrupoContacto).
        /// </summary>
        public Dictionary<string, string> ContactGroupMap { get; } = new()
        {
            [EnumContactabilityType.Phone.GetValueCatalog()] = "phone",
            [EnumContactabilityType.CommercialReference.GetValueCatalog()] = "phone",
            [EnumContactabilityType.WhatsApp.GetValueCatalog()] = "phone",
            [EnumContactabilityType.Email.GetValueCatalog()] = "email",
            [EnumContactabilityType.HomeAddress.GetValueCatalog()] = "address",
            [EnumContactabilityType.WorkAddress.GetValueCatalog()] = "address",
            [EnumContactabilityType.CommercialReference.GetValueCatalog()] = "reference",
            [EnumContactabilityType.PersonalReference.GetValueCatalog()] = "reference",
            [EnumContactabilityType.Emergency.GetValueCatalog()] = "emergency",
            [EnumContactabilityType.SocialNetwork.GetValueCatalog()] = "social",

        };

        public async Task<IActionResult> OnGetAsync()
        {
            if (!string.IsNullOrWhiteSpace(LoteIds))
                FilterIds = LoteIds;

            try
            {
                _logger.LogInformation("Dashboard accedido por el usuario: {User}", User.Identity?.Name);
                await LoadClientsAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar clientes en el dashboard");
                Clients = [];
                return Page();
            }
        }

        /// <summary>
        /// GET /Dashboard?handler=SearchByName&amp;q=texto
        /// Búsqueda ligera por nombre para el dropdown de sugerencias (TOP 10, vía caché).
        /// </summary>
        public async Task<IActionResult> OnGetSearchByNameAsync(string q, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 3)
                return new JsonResult(new { success = true, data = Array.Empty<object>() });

            try
            {
                IReadOnlyList<CustomerSearchResultDto> results = await _cacheService.SearchByNameAndIdentificationAsync(q.Trim(), ct);

                var data = results.Select(r => new
                {
                    id = r.Identification,
                    numericId = r.Id,
                    name = r.FullName,
                    initials = GetInitials(r.FullName),
                    status = r.IsVerified ? "Verificado" : "Por Verificar",
                });

                return new JsonResult(new { success = true, data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar clientes por nombre: {Q}", q);
                return new JsonResult(new { success = false, message = "Error interno" }) { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            _logger.LogInformation("El usuario {User} está cerrando sesión", User.Identity?.Name);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Login");
        }

        /// <summary>
        /// POST /Dashboard?handler=LoteBatch
        /// Recibe IDs del modal de lote, los guarda en TempData y redirige (PRG).
        /// </summary>
        public IActionResult OnPostLoteBatch([FromForm] string? ids)
        {
            if (string.IsNullOrWhiteSpace(ids))
                return RedirectToPage();

            const int MAX_IDS = 500;
            string[] parts = ids.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            LoteIds = string.Join(',', parts.Take(MAX_IDS));
            return RedirectToPage();
        }

        /// <summary>
        /// POST /Dashboard?handler=SearchClientsByIds
        /// Búsqueda batch por lista de cédulas. Llamado desde JS al procesar el modal de lote.
        /// </summary>
        public async Task<IActionResult> OnPostSearchClientsByIdsAsync()
        {
            try
            {
                using StreamReader reader = new(Request.Body);
                string body = await reader.ReadToEndAsync();

                SearchClientsByIdsRequest? searchData = JsonSerializer.Deserialize<SearchClientsByIdsRequest>(body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (searchData?.Ids == null || searchData.Ids.Count == 0)
                    return new JsonResult(new { success = false, message = "IDs no proporcionados" }) { StatusCode = 400 };

                List<ClientViewModel> clientesEncontrados = [];

                foreach (string id in searchData.Ids)
                {
                    if (!IsBasicValidId(id)) continue;
                    CustomerDto? cliente = await _queryService.ObtenerPorIdentificacionAsync(id);
                    if (cliente != null) clientesEncontrados.Add(ClientViewModelMapper.MapToViewModel(cliente));
                }

                _logger.LogInformation("Búsqueda por IDs: {Count} encontrados de {Total} solicitados",
                    clientesEncontrados.Count, searchData.Ids.Count);

                return new JsonResult(new
                {
                    success = true,
                    message = $"{clientesEncontrados.Count} cliente(s) encontrado(s)",
                    data = clientesEncontrados
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar clientes por IDs");
                return new JsonResult(new { success = false, message = "Error al procesar la solicitud: " + ex.Message }) { StatusCode = 500 };
            }
        }

        /// <summary>
        /// POST /Dashboard?handler=ValidateFileExtract
        /// Extrae IDs de un archivo Excel/CSV y devuelve el resultado como JSON.
        /// </summary>
        public async Task<IActionResult> OnPostValidateFileExtractAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return new JsonResult(new { isValid = false, errorMessage = "No se recibió ningún archivo." });

            try
            {
                FileExtractionResult result = await _fileExtractionService.ExtractAndValidateIdsAsync(file);
                return new JsonResult(new
                {
                    isValid = result.IsValid,
                    recordCount = result.RecordCount,
                    validIds = result.ValidIds,
                    identifierColumn = result.IdentifierColumn,
                    errorMessage = result.ErrorMessage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar el archivo del lote modal");
                return new JsonResult(new { isValid = false, errorMessage = "Error interno al procesar el archivo." }) { StatusCode = 500 };
            }
        }

        /// <summary>
        /// GET /Dashboard?handler=DownloadFileTemplate
        /// Descarga la plantilla Excel para carga masiva.
        /// </summary>
        public IActionResult OnGetDownloadFileTemplate()
        {
            string path = Path.Combine(_env.WebRootPath, "Template", "Plantilla_Consulta_Clientes.xlsx");
            if (!System.IO.File.Exists(path))
                return NotFound();

            return PhysicalFile(path,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Plantilla_Consulta_Clientes.xlsx");
        }

        // ─── Private helpers ──────────────────────────────────────────────────

        private async Task LoadClientsAsync()
        {
            if (string.IsNullOrWhiteSpace(FilterIds))
            {
                Clients = [];
                return;
            }

            string[] ids = FilterIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            List<ClientViewModel> result = [];

            foreach (string id in ids)
            {
                if (!IsBasicValidId(id)) continue;
                CustomerDto? cliente = await _queryService.ObtenerPorIdentificacionAsync(id);
                if (cliente != null) result.Add(ClientViewModelMapper.MapToViewModel(cliente));
            }

            Clients = result;
            _logger.LogInformation("Filtrado por IDs: {Found} encontrados de {Requested} solicitados",
                result.Count, ids.Length);
        }

        private static string GetInitials(string? name) => ClientViewModelMapper.GetInitials(name);

        private static bool IsBasicValidId(string id) =>
            !string.IsNullOrWhiteSpace(id) &&
            id.Length <= 50 &&
            System.Text.RegularExpressions.Regex.IsMatch(id, @"^[a-zA-Z0-9]+$");
    }

    public class SearchClientsByIdsRequest
    {
        public List<string> Ids { get; set; } = [];
    }
}
