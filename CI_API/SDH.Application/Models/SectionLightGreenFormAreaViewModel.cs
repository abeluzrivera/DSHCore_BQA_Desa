namespace SDH.Application.Models
{
    /// <summary>
    /// Estados posibles del componente de carga de archivos
    /// </summary>
    public enum FileUploadStatus
    {
        /// <summary>Estado inicial - sin archivo</summary>
        Empty = 0,

        /// <summary>Validando archivo...</summary>
        Loading = 1,

        /// <summary>Archivo válido - listo para procesar</summary>
        Success = 2,

        /// <summary>Error al procesar archivo</summary>
        Error = 3
    }

    /// <summary>
    /// ViewModel para el componente Section - Light Green Background Area for Form Content
    /// Contiene información sobre archivos cargados y registros detectados
    /// </summary>
    public class SectionLightGreenFormAreaViewModel
    {
        /// <summary>
        /// Estado actual del upload
        /// </summary>
        public FileUploadStatus Status { get; set; } = FileUploadStatus.Empty;

        /// <summary>
        /// Gets or sets the uploaded file information
        /// </summary>
        public FileUploadInfo? UploadedFile { get; set; }

        /// <summary>
        /// Gets or sets the number of records detected in the file
        /// </summary>
        public int RecordsDetected { get; set; }

        /// <summary>
        /// Mensaje de error (cuando Status = Error)
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Columna identificadora detectada
        /// </summary>
        public string IdentifierColumn { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether to show the alert box
        /// </summary>
        public bool ShowAlert { get; set; } = true;

        /// <summary>
        /// Obtiene si el estado actual es de error
        /// </summary>
        public bool IsError => Status == FileUploadStatus.Error;

        /// <summary>
        /// Obtiene si el estado actual es de éxito
        /// </summary>
        public bool IsSuccess => Status == FileUploadStatus.Success;

        /// <summary>
        /// Obtiene si se está cargando
        /// </summary>
        public bool IsLoading => Status == FileUploadStatus.Loading;

        /// <summary>
        /// Obtiene si está vacío
        /// </summary>
        public bool IsEmpty => Status == FileUploadStatus.Empty;
    }

    /// <summary>
    /// Represents information about an uploaded file
    /// </summary>
    public class FileUploadInfo
    {
        /// <summary>
        /// Gets or sets the file name including extension
        /// Example: "Base_Clientes_Octubre.xlsx"
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the formatted file size
        /// Example: "2.4 MB"
        /// </summary>
        public string FileSize { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file size in bytes (optional, for internal processing)
        /// </summary>
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// Gets or sets the file upload timestamp
        /// </summary>
        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }
}
