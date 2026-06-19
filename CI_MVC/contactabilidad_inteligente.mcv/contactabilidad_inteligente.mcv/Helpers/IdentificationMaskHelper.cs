namespace contactabilidad_inteligente.mcv.Helpers
{
    public static class IdentificationMaskHelper
    {
        // Oculta los dígitos en las posiciones 5 y 6 (índice 0), muestra el resto.
        // Ejemplo: "1234567890" → "12345##890"
        public static string Mask(string? identification)
        {
            if (string.IsNullOrEmpty(identification)) return string.Empty;

            char[] masked = identification.ToCharArray();

            if (identification.Length > 5) masked[5] = '#';
            if (identification.Length > 6) masked[6] = '#';

            return new string(masked);
        }
    }
}
