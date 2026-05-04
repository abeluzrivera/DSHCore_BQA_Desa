namespace contactabilidad_inteligente.mcv.Models.Client
{
    public class ContactViewModel
    {
        public long Id { get; set; }
        public string Icon { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string TipoContacto { get; set; } = string.Empty;
        public string BgClass { get; set; } = string.Empty;
        public string BorderClass { get; set; } = string.Empty;
        public string TextClass { get; set; } = string.Empty;
        public string ValueClass { get; set; } = string.Empty;
        public bool Verified { get; set; }
        /// <summary>"verified" | "pending" | "error"</summary>
        public string? State { get; set; } = "pending";
        /// <summary>"phone" | "email" | "location" | "link"</summary>
        public string IconType { get; set; } = "phone";
        /// <summary>Código de error (ej: "ERR_TEL_FMT") cuando State == "error", vacío en otros casos.</summary>
        public string ErrorCode { get; set; } = string.Empty;
        /// <summary>Etiqueta legible del error para mostrar en la UI.</summary>
        public string ErrorLabel { get; set; } = string.Empty;

        public int OrderPriority { get; set; }
    }
}
