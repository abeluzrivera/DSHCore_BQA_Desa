using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using SDH.Application.Ports.Services;
using System.Text.RegularExpressions;

namespace SDH.Application.Services
{
    public partial class FileExtractionService(ILogger<FileExtractionService> logger) : IFileExtractionService
    {
        private static readonly long MaxBytes = 10 * 1024 * 1024; // 10 MB
        private static readonly string[] AllowedExtensions = [".xlsx", ".xls", ".csv"];
        private static readonly string[] HeaderCandidates = ["identificacion", "cedula", "id", "identificación", "ruc", "identificador"];

        private readonly ILogger<FileExtractionService> _logger = logger;

        public async Task<FileExtractionResult> ExtractAndValidateIdsAsync(IFormFile file)
        {
            // LOG ESTRUCTURADO: Registramos el intento con variables (facilita la búsqueda en Kibana/Datadog)
            _logger.LogInformation("Iniciando extracción. Archivo: {FileName}, Tamaño: {FileSize} bytes", file?.FileName, file?.Length);

            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("Rechazo de archivo: El archivo enviado está vacío o es nulo.");
                return Fail("El archivo está vacío.");
            }

            if (file.Length > MaxBytes)
            {
                _logger.LogWarning("Rechazo de archivo: {FileName} supera el límite permitido ({FileSize} > {MaxBytes}).", file.FileName, file.Length, MaxBytes);
                return Fail("El archivo supera el límite de 10 MB.");
            }

            string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
            {
                _logger.LogWarning("Rechazo de archivo: Extensión no permitida '{Extension}' en el archivo {FileName}.", ext, file.FileName);
                return Fail($"Formato no permitido: {ext}. Use .xlsx, .xls o .csv.");
            }

            try
            {
                return ext == ".csv"
                    ? await ExtractFromCsvAsync(file)
                    : await ExtractFromExcelAsync(file);
            }
            catch (Exception ex)
            {
                // LOG DE ERROR: Pasamos la excepción completa como primer parámetro para capturar el StackTrace
                _logger.LogError(ex, "Error crítico al procesar el archivo {FileName}", file.FileName);
                return Fail($"Error al procesar el archivo: {ex.Message}");
            }
        }

        // ── CSV extraction ──────────────────────────────────────────────────────
        private async Task<FileExtractionResult> ExtractFromCsvAsync(IFormFile file)
        {
            _logger.LogDebug("Iniciando lectura de CSV para {FileName}", file.FileName);

            using StreamReader reader = new(file.OpenReadStream(), detectEncodingFromByteOrderMarks: true);

            string? headerLine = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(headerLine))
            {
                _logger.LogWarning("Lectura CSV fallida: {FileName} no tiene cabeceras o está vacío.", file.FileName);
                return Fail("El archivo CSV parece estar vacío.");
            }

            string[] headers = headerLine.Split([',', ';', '\t'], StringSplitOptions.None);
            var (idColumnIndex, identifierColumn) = DetectIdentifierColumn(headers);

            if (idColumnIndex < 0) return FailColumnNotFound(file.FileName);

            List<string> rawIds = [];
            while (!reader.EndOfStream)
            {
                string? line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] cols = line.Split([',', ';', '\t'], StringSplitOptions.None);
                if (cols.Length > idColumnIndex)
                {
                    string value = cols[idColumnIndex].Trim().Trim('"');
                    if (!string.IsNullOrWhiteSpace(value)) rawIds.Add(value);
                }
            }

            return BuildResult(rawIds, identifierColumn, file.FileName);
        }

        // ── Excel extraction with EPPlus ─────────────────────────────────────────
        private async Task<FileExtractionResult> ExtractFromExcelAsync(IFormFile file)
        {
            _logger.LogDebug("Iniciando lectura de Excel (EPPlus) para {FileName}", file.FileName);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using Stream stream = file.OpenReadStream();
            using ExcelPackage package = new(stream);

            ExcelWorksheet? sheet = package.Workbook.Worksheets.FirstOrDefault();
            if (sheet == null)
            {
                _logger.LogWarning("Lectura Excel fallida: {FileName} no contiene hojas.", file.FileName);
                return Fail("El archivo Excel no contiene hojas.");
            }

            int totalCols = sheet.Dimension?.Columns ?? 0;
            if (totalCols == 0) return Fail("El archivo Excel está vacío.");

            string[] headers = new string[totalCols];
            for (int col = 1; col <= totalCols; col++)
            {
                headers[col - 1] = sheet.Cells[1, col].Text?.Trim() ?? "";
            }

            var (idColumnIndex, identifierColumn) = DetectIdentifierColumn(headers);
            if (idColumnIndex < 0) return FailColumnNotFound(file.FileName);

            List<string> rawIds = [];
            int lastRow = sheet.Dimension?.Rows ?? 1;
            int epplusColIndex = idColumnIndex + 1;

            for (int row = 2; row <= lastRow; row++)
            {
                string? cell = sheet.Cells[row, epplusColIndex].Text?.Trim();
                if (!string.IsNullOrWhiteSpace(cell)) rawIds.Add(cell);
            }

            return await Task.FromResult(BuildResult(rawIds, identifierColumn, file.FileName));
        }

        // ── Helpers ──────────────────────────────────────────────────────────────
        private static (int Index, string? ColumnName) DetectIdentifierColumn(string[] headers)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                string h = headers[i].Trim().Trim('"').ToLowerInvariant();
                if (HeaderCandidates.Any(c => h.Contains(c))) return (i, headers[i].Trim().Trim('"'));
            }
            return (-1, null);
        }

        // Añadí el parámetro fileName para mejorar el contexto del log
        private FileExtractionResult BuildResult(List<string> rawIds, string? identifierColumn, string fileName)
        {
            if (rawIds.Count == 0)
            {
                _logger.LogWarning("Proceso completado sin datos: No se encontraron valores en la columna '{Column}' del archivo {FileName}.", identifierColumn, fileName);
                return Fail("No se encontraron identificadores en el archivo.");
            }

            List<string> validIds = rawIds
                .Where(IsValidId)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (validIds.Count == 0)
            {
                _logger.LogWarning("Proceso completado con rechazo total: De {TotalRows} filas extraídas en {FileName}, ninguna cumplió el formato de ID.", rawIds.Count, fileName);
                return Fail("Ninguno de los identificadores encontrados tiene un formato válido.");
            }

            // LOG DE NEGOCIO: Métricas clave para análisis
            _logger.LogInformation("Extracción exitosa en {FileName}. Extraídos: {TotalRows}. Válidos y únicos: {ValidCount}. Descartados: {DiscardedCount}.",
                fileName, rawIds.Count, validIds.Count, rawIds.Count - validIds.Count);

            return new FileExtractionResult
            {
                IsValid = true,
                RecordCount = validIds.Count,
                ValidIds = validIds,
                IdentifierColumn = identifierColumn,
            };
        }

        private static bool IsValidId(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            string v = value.Trim();
            return RegexNumeric().IsMatch(v) || RegexAlphanumeric().IsMatch(v);
        }

        private static FileExtractionResult Fail(string message) => new() { IsValid = false, ErrorMessage = message };

        private FileExtractionResult FailColumnNotFound(string fileName)
        {
            _logger.LogWarning("Falta de cabeceras requeridas: No se encontró una columna de ID en {FileName}. Esperado alguna de: {Candidates}", fileName, string.Join(", ", HeaderCandidates));
            return Fail("No se encontró columna de identificación. Use: identificacion, cedula, id, ruc o identificador.");
        }

        [GeneratedRegex(@"^\d{9,10}$|^\d{13}$", RegexOptions.Compiled)]
        private static partial Regex RegexNumeric();

        [GeneratedRegex(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z0-9]{6,}$", RegexOptions.Compiled)]
        private static partial Regex RegexAlphanumeric();
    }
}