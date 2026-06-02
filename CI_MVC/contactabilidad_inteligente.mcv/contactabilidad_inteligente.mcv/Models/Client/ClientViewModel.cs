namespace contactabilidad_inteligente.mcv.Models.Client
{
    public class ClientViewModel
    {
        /// <summary>ID numérico de BD — usado en rutas de la API (/api/clientes/{NumericId}/...).</summary>
        public long NumericId { get; set; }
        /// <summary>Cédula/identificación del cliente — usado como clave pública en el DOM.</summary>
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string AvatarBgClass { get; set; } = "bg-forest-green/10";
        public string? AvatarUrl { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusClass { get; set; } = string.Empty;
        public string? Location { get; set; }
        public List<ContactViewModel> Contacts { get; set; } = [];
    }
}
