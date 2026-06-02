namespace SDH.Domain.Entities.Parametro
{
    /// <summary>
    /// Aggregate Root: Representa un grupo de catálogo que agrupa ítems relacionados (ej: Estados, Tipos, etc.)
    /// </summary>
    public class CatalogGroup
    {
        public int Id { get; private set; } // Renombrado a Id
        public string GroupName { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public bool IsSystem { get; private set; }

        public DateTime CreatedAt { get; private set; }
        public string? CreatedBy { get; private set; }
        public DateTime? LastModifiedAt { get; private set; }
        public string? LastModifiedBy { get; private set; }

        // Backing field: Mantiene el encapsulamiento de la colección
        private readonly List<CatalogItem> _items = [];
        public IReadOnlyCollection<CatalogItem> Items => _items.AsReadOnly();

        // Constructor privado para EF Core
        private CatalogGroup() { }

        // Factory method
        public static CatalogGroup Create(
            string nombreGrupo,
            string? descripcion = null,
            bool esSistema = false,
            string? usuarioCreacion = null)
        {
            return new CatalogGroup
            {
                GroupName = nombreGrupo,
                Description = descripcion,
                IsSystem = esSistema,
                CreatedAt = DateTime.Now, // Siempre Now
                CreatedBy = TruncateString(usuarioCreacion, 50)
            };
        }

        // ──────────────────────────────────────────────────
        // COMPORTAMIENTOS DEL NEGOCIO
        // ──────────────────────────────────────────────────

        public void UpdateDescription(string descripcion, string? usuarioModificacion)
        {
            // ¡Regla de negocio pura!
            if (IsSystem)
            {
                throw new InvalidOperationException("No se puede modificar la descripción de un catálogo del sistema.");
            }

            Description = descripcion;
        }

        public void AddItem(CatalogItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            _items.Add(item);
        }

        // Helper privado
        private static string? TruncateString(string? valor, int maxLenght)
        {
            if (string.IsNullOrEmpty(valor)) return valor;
            return valor.Length > maxLenght ? valor[..maxLenght] : valor;
        }
    }
}