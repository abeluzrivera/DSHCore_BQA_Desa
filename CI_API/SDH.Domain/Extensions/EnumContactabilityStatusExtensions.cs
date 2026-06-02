using SDH.Domain.Enums;

namespace SDH.Domain.Extensions
{
    public static class EnumContactabilityStatusExtensions
    {
        /// <summary>
        /// Evalúa si el valor del Enum se considera un estado de error
        /// (Cualquier cosa que no sea Verified ni Pending).
        /// </summary>
        public static bool IsError(this EnumContactabilityStatus status)
        {
            return status != EnumContactabilityStatus.Verified &&
                   status != EnumContactabilityStatus.Pending;
        }

        public static bool IsError(int? codigo)
        {

            return codigo != (int)EnumContactabilityStatus.Verified &&
                   codigo != (int)EnumContactabilityStatus.Pending;
        }

        /// <summary>
        /// Mantiene la compatibilidad exacta con tu función anterior si 
        /// todavía necesitas evaluar el código crudo de base de datos (string).
        /// </summary>
        public static bool IsError(string? codigo)
        {
            if (string.IsNullOrEmpty(codigo)) return false;

            // Usamos pattern matching de C# moderno para mayor legibilidad y rendimiento
            return codigo is not "VERIF" and not "PEND";
        }
    }
}
