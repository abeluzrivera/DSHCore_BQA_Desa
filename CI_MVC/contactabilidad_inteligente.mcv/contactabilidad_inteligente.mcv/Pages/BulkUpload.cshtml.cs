using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SDH.Application.Models;
using SDH.Application.Services;

namespace contactabilidad_inteligente.mcv.Pages
{
    [Authorize]
    public class BulkUploadModel : PageModel
    {
        private readonly ILogger<BulkUploadModel> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly IFileValidationService _fileValidationService;

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        [BindProperty]
        public SectionLightGreenFormAreaViewModel FileUploadViewModel { get; set; } = new();

        public BulkUploadModel(ILogger<BulkUploadModel> logger, IWebHostEnvironment environment, IFileValidationService fileValidationService = null)
        {
            _logger = logger;
            _environment = environment;
            _fileValidationService = fileValidationService;
        }

        public IActionResult OnGet()
        {
            // Módulo en desarrollo — redirigir al dashboard
            return RedirectToPage("/Dashboard");
        }

        public async Task<IActionResult> OnPostValidateFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                FileUploadViewModel.Status = FileUploadStatus.Error;
                FileUploadViewModel.ErrorMessage = "Por favor selecciona un archivo.";
                return Page();
            }

            if (_fileValidationService == null)
            {
                FileUploadViewModel.Status = FileUploadStatus.Error;
                FileUploadViewModel.ErrorMessage = "Servicio de validación no disponible. Por favor intente más tarde.";
                return Page();
            }

            FileValidationResult result = await _fileValidationService.ValidateExcelFileAsync(file);

            if (!result.IsValid)
            {
                FileUploadViewModel.Status = FileUploadStatus.Error;
                FileUploadViewModel.ErrorMessage = result.ErrorMessage;
                _logger.LogWarning("Validación fallida para archivo {FileName}: {Error}", file.FileName, result.ErrorMessage);
                return Page();
            }

            // Archivo válido
            FileUploadViewModel.Status = FileUploadStatus.Success;
            FileUploadViewModel.UploadedFile = new()
            {
                FileName = result.FileName,
                FileSizeBytes = result.FileSize,
                UploadedAt = DateTime.Now
            };
            FileUploadViewModel.RecordsDetected = result.RecordCount;
            FileUploadViewModel.IdentifierColumn = result.IdentifierColumn;

            _logger.LogInformation("Archivo validado exitosamente: {FileName}, Registros: {Records}", result.FileName, result.RecordCount);
            return Page();
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            // Registrar intento de cierre de sesi�n
            _logger.LogInformation("El usuario {User} est� cerrando sesi�n desde BulkUpload", User.Identity?.Name);

            // Cerrar sesi�n
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirigir a la p�gina de inicio de sesi�n
            return RedirectToPage("/Auth/Login");
        }

        public IActionResult OnGetDownloadTemplate()
        {
            try
            {
                // Intentar m�ltiples rutas posibles
                string[] templatePaths = new[]
                {
                    Path.Combine(_environment.WebRootPath, "Template", "Plantilla_Contactabilidad_Cliente.xlsx"),
                    Path.Combine(_environment.ContentRootPath, "wwwroot", "Template", "Plantilla_Contactabilidad_Cliente.xlsx"),
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Template", "Plantilla_Contactabilidad_Cliente.xlsx")
                };

                string? templatePath = null;
                foreach (string? path in templatePaths)
                {
                    if (System.IO.File.Exists(path))
                    {
                        templatePath = path;
                        _logger.LogInformation("Plantilla encontrada en: {Path}", path);
                        break;
                    }
                }

                if (templatePath == null)
                {
                    _logger.LogWarning("Plantilla no encontrada en ninguna de las rutas intentadas");
                    _logger.LogWarning("WebRootPath: {WebRootPath}", _environment.WebRootPath);
                    _logger.LogWarning("ContentRootPath: {ContentRootPath}", _environment.ContentRootPath);
                    return NotFound("La plantilla no est� disponible. Por favor, contacte al administrador.");
                }

                byte[] fileBytes = System.IO.File.ReadAllBytes(templatePath);
                string fileName = "Plantilla_Contactabilidad_Cliente.xlsx";

                _logger.LogInformation("Descargando plantilla ({Size} bytes) por: {User}", fileBytes.Length, User.Identity?.Name);

                return File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName
                );
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Error de I/O al descargar plantilla");
                return StatusCode(500, "Error al descargar la plantilla.");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Acceso denegado al descargar plantilla");
                return StatusCode(500, "Error al descargar la plantilla.");
            }
        }

        public async Task<IActionResult> OnPostUploadAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return new JsonResult(new { success = false, message = "No se ha seleccionado ning�n archivo" });
            }

            // Validar extensi�n
            string[] allowedExtensions = new[] { ".xlsx", ".xls", ".csv" };
            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return new JsonResult(new { success = false, message = "Formato de archivo no v�lido. Solo se permiten archivos Excel (.xlsx, .xls) o CSV (.csv)" });
            }

            // Validar tama�o (25MB max)
            if (file.Length > 25 * 1024 * 1024)
            {
                return new JsonResult(new { success = false, message = "El archivo excede el tama�o m�ximo de 25MB" });
            }

            try
            {
                _logger.LogInformation("Procesando archivo: {FileName} ({Size} bytes) por usuario: {User}",
                    file.FileName, file.Length, User.Identity?.Name);

                // Simular procesamiento progresivo
                await Task.Delay(3000);

                // Aqu� ir�a la l�gica real de procesamiento del archivo
                // Por ahora simulamos resultados
                int totalRecords = 500;
                int successCount = 482;
                int errorCount = 18;

                // Simular errores espec�ficos
                var errors = new[]
                {
                    new { row = 45, field = "Email", message = "Formato de email inv�lido" },
                    new { row = 82, field = "Tel�fono", message = "Faltan d�gitos requeridos" },
                    new { row = 103, field = "RUT", message = "D�gito verificador incorrecto" },
                    new { row = 156, field = "C�digo Postal", message = "Valor no num�rico" },
                    new { row = 210, field = "Email", message = "Dominio bloqueado" },
                    new { row = 287, field = "Tel�fono", message = "Formato inv�lido" },
                    new { row = 324, field = "RUT", message = "RUT duplicado en el sistema" },
                    new { row = 389, field = "Email", message = "Email ya registrado" },
                    new { row = 421, field = "Nombre", message = "Campo requerido vac�o" },
                    new { row = 445, field = "Direcci�n", message = "Direcci�n demasiado larga" },
                    new { row = 467, field = "Tel�fono", message = "N�mero no v�lido para Chile" },
                    new { row = 478, field = "Email", message = "Formato de email inv�lido" },
                    new { row = 489, field = "C�digo Postal", message = "C�digo postal no existe" },
                    new { row = 491, field = "RUT", message = "Formato de RUT inv�lido" },
                    new { row = 493, field = "Email", message = "Dominio no v�lido" },
                    new { row = 496, field = "Tel�fono", message = "Prefijo internacional incorrecto" },
                    new { row = 498, field = "Nombre", message = "Contiene caracteres no permitidos" },
                    new { row = 499, field = "Email", message = "Longitud m�xima excedida" }
                };

                var result = new
                {
                    success = true,
                    fileName = file.FileName,
                    fileSize = FormatFileSize(file.Length),
                    totalRecords = totalRecords,
                    recordsProcessed = totalRecords,
                    successCount = successCount,
                    errorCount = errorCount,
                    errors = errors
                };

                _logger.LogInformation("Archivo procesado exitosamente: {FileName}, �xitos: {Success}, Errores: {Errors}",
                    file.FileName, successCount, errorCount);

                return new JsonResult(result);
            }
            catch (InvalidDataException ex)
            {
                _logger.LogError(ex, "Datos inválidos en archivo de carga masiva: {FileName}", file.FileName);
                return new JsonResult(new { success = false, message = "El archivo contiene datos inválidos. Por favor, intente nuevamente." });
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Error de I/O al procesar archivo de carga masiva: {FileName}", file.FileName);
                return new JsonResult(new { success = false, message = "Error al procesar el archivo. Por favor, intente nuevamente." });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error de operación al procesar archivo de carga masiva: {FileName}", file.FileName);
                return new JsonResult(new { success = false, message = "Error al procesar el archivo. Por favor, intente nuevamente." });
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = ["B", "KB", "MB", "GB"];
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        public IActionResult OnGetDownloadErrorLog(string fileName, string errors)
        {
            try
            {
                if (string.IsNullOrEmpty(errors))
                {
                    return BadRequest("No hay errores para descargar");
                }

                // Decodificar el JSON de errores
                List<ErrorDetail>? errorsList = System.Text.Json.JsonSerializer.Deserialize<List<ErrorDetail>>(errors);

                if (errorsList == null || errorsList.Count == 0)
                {
                    return BadRequest("No hay errores para descargar");
                }

                // Create CSV
                System.Text.StringBuilder csv = new();
                csv.AppendLine("\uFEFF"); // BOM para UTF-8
                csv.AppendLine("FILA,CAMPO,MOTIVO DEL ERROR");

                foreach (ErrorDetail error in errorsList)
                {
                    csv.AppendLine($"{error.Row},{error.Field},\"{error.Message}\"");
                }

                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string logFileName = $"Log_Errores_{fileName}_{timestamp}.csv";

                _logger.LogInformation("Descargando log de errores por: {User}", User.Identity?.Name);

                return File(bytes, "text/csv", logFileName);
            }
            catch (System.Text.Json.JsonException ex)
            {
                _logger.LogError(ex, "Error al deserializar errores para el log");
                return StatusCode(500, "Error al generar el archivo de log");
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Error de I/O al generar log de errores");
                return StatusCode(500, "Error al generar el archivo de log");
            }
        }

        private class ErrorDetail
        {
            public int Row { get; set; }
            public string Field { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
        }
    }
}
