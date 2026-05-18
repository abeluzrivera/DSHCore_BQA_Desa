using SDH.Domain.Enums;
using SDH.Domain.Extensions;

namespace contactabilidad_inteligente.mcv.Helpers
{
    public static class ContactabilityLabelHelper
    {
        // ═══════════════════════════════════════════════════════════════════
        // CATÁLOGO DE ERRORES (Error Codes)
        // ═══════════════════════════════════════════════════════════════════

        public static readonly Dictionary<string, List<(string Code, string ShortLabel)>> RejectionPills = new()
        {
            ["email"] = [
                (EnumContactabilityStatus.EmailFormato.GetValueCatalog(), "Formato no válido"),
                (EnumContactabilityStatus.EmailDominio.GetValueCatalog(), "Dominio inalcanzable"),
                (EnumContactabilityStatus.EmailTemporal.GetValueCatalog(), "Correo desechable"),
                (EnumContactabilityStatus.EmailDuplicado.GetValueCatalog(), "Ya registrado"),
                (EnumContactabilityStatus.EmailRebote.GetValueCatalog(), "Rebote anterior")
            ],
            ["phone"] = [
                (EnumContactabilityStatus.TelFormato.GetValueCatalog(), "Formato no válido"),
                (EnumContactabilityStatus.TelDigitos.GetValueCatalog(), "Dígitos incorrectos"),
                (EnumContactabilityStatus.TelNoNumerico.GetValueCatalog(), "Contiene letras"),
                (EnumContactabilityStatus.TelCodigo.GetValueCatalog(), "Código inválido"),
                (EnumContactabilityStatus.TelFicticio.GetValueCatalog(), "Ficticio/Repetido")
            ],
            ["address"] = [
                (EnumContactabilityStatus.DirIncompleta.GetValueCatalog(), "Incompleta"),
                (EnumContactabilityStatus.DirCorta.GetValueCatalog(), "Demasiado corta"),
                (EnumContactabilityStatus.DirGenerica.GetValueCatalog(), "Dirección genérica"),
                (EnumContactabilityStatus.DirCaracteres.GetValueCatalog(), "Caracteres inválidos"),
                (EnumContactabilityStatus.DirGeoInconsistente.GetValueCatalog(), "Inconsistencia geo")
            ]
        };

        private static readonly Dictionary<string, string> _flatPillsLookup = RejectionPills.Values
            .SelectMany(lista => lista)
            .ToDictionary(pill => pill.Code, pill => pill.ShortLabel);

        // Labels for generic statuses that are not rejection pills
        private static readonly Dictionary<string, string> _genericStatusLabels = new()
        {
            ["NORES"]       = "Sin respuesta",
            ["CADU"]        = "Información vencida",
            ["ERROR"]       = "Error de validación",
            ["PEND-LOPDP"]  = "Pendiente LOPDP",
            ["Deny-LOPDP"]  = "Rechazado LOPDP",
        };

        public static string GetErrorLabelContactability(string errorCode)
        {
            if (_flatPillsLookup.TryGetValue(errorCode, out string? shortLabel))
                return shortLabel;

            if (_genericStatusLabels.TryGetValue(errorCode, out string? genericLabel))
                return genericLabel;

            return errorCode;
        }

        // ═══════════════════════════════════════════════════════════════════
        // CATÁLOGO DE TIPOS DE CONTACTO (Contact Types Metadata)
        // ═══════════════════════════════════════════════════════════════════

        public record ContactTypeMetadata(
            string Code,
            string Icon,
            string IconType,
            string Label,
            int Order,
            bool ShouldTruncate
        );

        public static readonly Dictionary<EnumContactabilityType, ContactTypeMetadata> ContactTypes = new()
        {
            [EnumContactabilityType.Phone] =
                new(EnumContactabilityType.Phone.GetValueCatalog(), "smartphone", "phone", "Celular", 0, false),

            [EnumContactabilityType.Conventional] =
                new(EnumContactabilityType.Conventional.GetValueCatalog(), "phone", "phone", "Teléfono", 1, false),

            [EnumContactabilityType.WhatsApp] =
                new(EnumContactabilityType.WhatsApp.GetValueCatalog(), "chat", "phone", "WhatsApp", 2, false),

            [EnumContactabilityType.Email] =
                new(EnumContactabilityType.Email.GetValueCatalog(), "mail", "email", "Email", 3, true),

            [EnumContactabilityType.WorkAddress] =
                new(EnumContactabilityType.WorkAddress.GetValueCatalog(), "business", "location", "Dir. Trabajo", 4, true),

            [EnumContactabilityType.HomeAddress] =
                new(EnumContactabilityType.HomeAddress.GetValueCatalog(), "home", "location", "Dir. Domicilio", 5, true),

            [EnumContactabilityType.SocialNetwork] =
                new(EnumContactabilityType.SocialNetwork.GetValueCatalog(), "link", "link", "LinkedIn", 6, true)
        };

        // ═══════════════════════════════════════════════════════════════════
        // CATÁLOGO DE ESTADOS (Contact States Metadata)
        // ═══════════════════════════════════════════════════════════════════

        public record StateMetadata(
            string Code,
            string BgClass,
            string BorderClass,
            string TextClass,
            string State
        );

        public static readonly Dictionary<EnumContactabilityStatus, StateMetadata> States = new()
        {
            [EnumContactabilityStatus.Verified] = new(
                EnumContactabilityStatus.Verified.GetValueCatalog(),
                "tw-bg-verified-bg",
                "tw-border-verified-border",
                "tw-text-verified-text",
                "verified"
            ),

            [EnumContactabilityStatus.Pending] = new(
                EnumContactabilityStatus.Pending.GetValueCatalog(),
                "tw-bg-pending-bg",
                "tw-border-pending-border",
                "tw-text-pending-text",
                "pending"
            ),

            [EnumContactabilityStatus.Error] = new(
                EnumContactabilityStatus.Error.GetValueCatalog(),
                "tw-bg-unverified-bg",
                "tw-border-unverified-border",
                "tw-text-unverified-text",
                "error"
            )
        };
    }
}
