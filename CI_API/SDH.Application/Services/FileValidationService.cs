using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;

namespace SDH.Application.Services
{
    public interface IFileValidationService
    {
        Task<FileValidationResult> ValidateExcelFileAsync(IFormFile file);
        Task<FileValidationResult> ValidateRequiredColumnsAsync(IFormFile file);
    }

    public class FileValidationService(ILogger<FileValidationService> logger) : IFileValidationService
    {
        private static readonly string[] AllowedExtensions = [".xlsx", ".xls", ".csv"];
        private const long MaxFileSizeBytes = 25 * 1024 * 1024; // 25 MB

        // Unificamos las constantes para que coincidan con la extracción
        private static readonly string[] IdentifierColumnCandidates = ["identificacion", "cedula", "id", "identificación", "ruc", "identificador"];

        private readonly ILogger<FileValidationService> _logger = logger;

        public async Task<FileValidationResult> ValidateExcelFileAsync(IFormFile file)
        {
            _logger.LogInformation("Iniciando validación de archivo: {FileName}, Tamaño: {FileSize} bytes", file?.FileName, file?.Length);

            try
            {
                // 1. Validaciones base (Early Returns)
                var baseValidation = ValidateFileBasics(file);
                if (!baseValidation.IsValid) return baseValidation;

                // 2. Validar estructura y obtener columnas (Lectura real)
                var columnValidation = await ValidateRequiredColumnsAsync(file!);
                if (!columnValidation.IsValid) return columnValidation;

                _logger.LogInformation("Archivo {FileName} validado exitosamente. Columnas detectadas: {ColumnsCount}", file!.FileName, columnValidation.DetectedColumns.Count);

                // 3. Ensamblar resultado exitoso
                return new FileValidationResult
                {
                    IsValid = true,
                    FileName = file.FileName,
                    FileSize = file.Length,
                    IdentifierColumn = columnValidation.IdentifierColumn,
                    DetectedColumns = columnValidation.DetectedColumns,
                    RecordCount = columnValidation.RecordCount // Obtenido durante la lectura
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado validando el archivo {FileName}", file?.FileName);
                return Fail(file, $"Error al procesar el archivo: {ex.Message}");
            }
        }

        public async Task<FileValidationResult> ValidateRequiredColumnsAsync(IFormFile file)
        {
            if (file == null) return Fail(file, "El archivo es nulo.");

            string ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            try
            {
                return ext == ".csv"
                    ? await ValidateCsvHeadersAsync(file)
                    : await ValidateExcelHeadersAsync(file);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extrayendo cabeceras del archivo {FileName}", file.FileName);
                return Fail(file, $"Error al leer la estructura del archivo: {ex.Message}");
            }
        }

        // ── Validaciones Privadas ──────────────────────────────────────────────

        private FileValidationResult ValidateFileBasics(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("Validación fallida: Archivo vacío o nulo.");
                return Fail(file, "Por favor selecciona un archivo.");
            }

            string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
            {
                _logger.LogWarning("Validación fallida: Extensión no permitida '{Ext}' en {FileName}", ext, file.FileName);
                return Fail(file, $"Tipo de archivo no válido. Se aceptan: {string.Join(", ", AllowedExtensions)}.");
            }

            if (file.Length > MaxFileSizeBytes)
            {
                double sizeInMb = file.Length / (1024.0 * 1024.0);
                _logger.LogWarning("Validación fallida: Archivo demasiado pesado ({SizeInMb} MB) en {FileName}", sizeInMb, file.FileName);
                return Fail(file, $"El archivo excede el tamaño máximo de 25 MB.");
            }

            return new FileValidationResult { IsValid = true };
        }

        private async Task<FileValidationResult> ValidateCsvHeadersAsync(IFormFile file)
        {
            using StreamReader reader = new(file.OpenReadStream(), detectEncodingFromByteOrderMarks: true);

            string? headerLine = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(headerLine))
                return Fail(file, "El archivo CSV está vacío o no tiene cabeceras.");

            List<string> headers = headerLine.Split([',', ';', '\t'], StringSplitOptions.None)
                                             .Select(h => h.Trim().Trim('"'))
                                             .ToList();

            string? idColumn = DetectIdentifier(headers);

            // Estimación rápida de filas (opcional, solo lee hasta 1000 para no bloquear la validación)
            int rowCount = 0;
            while (!reader.EndOfStream && rowCount < 1000)
            {
                await reader.ReadLineAsync();
                rowCount++;
            }

            if (idColumn == null)
                return FailColumnNotFound(file, headers);

            return SuccessWithHeaders(file, headers, idColumn, rowCount);
        }

        private async Task<FileValidationResult> ValidateExcelHeadersAsync(IFormFile file)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using Stream stream = file.OpenReadStream();
            using ExcelPackage package = new(stream);

            ExcelWorksheet? sheet = package.Workbook.Worksheets.FirstOrDefault();
            if (sheet == null) return Fail(file, "El archivo Excel no contiene hojas.");

            int totalCols = sheet.Dimension?.Columns ?? 0;
            int totalRows = sheet.Dimension?.Rows ?? 0;

            if (totalCols == 0 || totalRows == 0)
                return Fail(file, "El archivo Excel está vacío.");

            List<string> headers = new(totalCols);
            for (int col = 1; col <= totalCols; col++)
            {
                headers.Add(sheet.Cells[1, col].Text?.Trim() ?? $"Columna_{col}");
            }

            string? idColumn = DetectIdentifier(headers);

            if (idColumn == null)
                return FailColumnNotFound(file, headers);

            // Descontamos 1 por la fila de cabecera
            return SuccessWithHeaders(file, headers, idColumn, totalRows - 1);
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static string? DetectIdentifier(List<string> headers)
        {
            return headers.FirstOrDefault(h =>
                IdentifierColumnCandidates.Any(c => h.ToLowerInvariant().Contains(c))
            );
        }

        private FileValidationResult Fail(IFormFile? file, string message)
        {
            return new FileValidationResult
            {
                IsValid = false,
                FileName = file?.FileName ?? "Desconocido",
                FileSize = file?.Length ?? 0,
                ErrorMessage = message
            };
        }

        private FileValidationResult FailColumnNotFound(IFormFile file, List<string> detectedHeaders)
        {
            _logger.LogWarning("Validación de columnas fallida para {FileName}. Columnas encontradas: {Headers}", file.FileName, string.Join(", ", detectedHeaders));
            return Fail(file, $"No se detectó una columna identificadora válida. Esperada alguna de: {string.Join(", ", IdentifierColumnCandidates)}.");
        }

        private FileValidationResult SuccessWithHeaders(IFormFile file, List<string> headers, string idColumn, int recordCount)
        {
            return new FileValidationResult
            {
                IsValid = true,
                FileName = file.FileName,
                FileSize = file.Length,
                DetectedColumns = headers,
                IdentifierColumn = idColumn,
                RecordCount = recordCount
            };
        }
    }

    // Nota: El DTO FileValidationResult se mantiene intacto ya que la propiedad FormattedFileSize es muy útil.
    public class FileValidationResult
    {
        public bool IsValid { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public int RecordCount { get; set; }
        public string? IdentifierColumn { get; set; }
        public List<string> DetectedColumns { get; set; } = [];
        public string? ErrorMessage { get; set; }

        public string FormattedFileSize
        {
            get
            {
                string[] sizes = ["B", "KB", "MB", "GB"];
                double len = FileSize;
                int order = 0;

                while (len >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    len /= 1024;
                }

                return $"{len:0.##} {sizes[order]}";
            }
        }
    }
}