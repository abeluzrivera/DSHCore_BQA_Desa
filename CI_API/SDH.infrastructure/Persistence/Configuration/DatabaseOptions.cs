namespace SDH.infrastructure.Persistence.Configuration
{
    /// <summary>
    /// Opciones de configuración para la base de datos
    /// </summary>
    public class DatabaseOptions
    {
        /// <summary>
        /// Nombres de la sección de configuración
        /// </summary>
        public const string SectionName = "ConnectionStrings";

        /// <summary>
        /// Nombres de la conexión por defecto
        /// </summary>
        public const string DefaultConnectionName = "DefaultConnection";

        /// <summary>
        /// Cadena de conexión
        /// </summary>
        public string DefaultConnection { get; set; } = string.Empty;

        /// <summary>
        /// Indica si se deben aplicar migraciones automáticamente (solo para desarrollo)
        /// </summary>
        public bool AutoMigrate { get; set; } = false;

        /// <summary>
        /// Timeout de comando en segundos
        /// </summary>
        public int CommandTimeout { get; set; } = 30;

        /// <summary>
        /// Habilitar logging detallado de EF Core
        /// </summary>
        public bool EnableSensitiveDataLogging { get; set; } = false;

        /// <summary>
        /// Habilitar logging detallado de SQL
        /// </summary>
        public bool EnableDetailedErrors { get; set; } = false;
    }
}
