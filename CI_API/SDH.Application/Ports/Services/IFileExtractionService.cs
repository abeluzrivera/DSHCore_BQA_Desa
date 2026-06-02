using Microsoft.AspNetCore.Http;

namespace SDH.Application.Ports.Services
{
    /// <summary>
    /// Extrae identificadores desde un archivo Excel o CSV subido por el usuario
    /// y los valida contra las reglas de negocio.
    /// </summary>
    public interface IFileExtractionService
    {
        /// <summary>
        /// Lee el archivo, detecta la columna de identificadores, extrae y valida cada ID.
        /// </summary>
        Task<FileExtractionResult> ExtractAndValidateIdsAsync(IFormFile file);
    }

    /// <summary>
    /// Resultado de la extracción y validación de IDs desde un archivo.
    /// </summary>
    public class FileExtractionResult
    {
        public bool IsValid { get; set; }
        public int RecordCount { get; set; }
        public List<string> ValidIds { get; set; } = [];
        public string? IdentifierColumn { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
