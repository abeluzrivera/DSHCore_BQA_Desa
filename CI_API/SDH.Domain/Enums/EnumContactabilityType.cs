using SDH.Domain.Attributes; // Ajusta este using a donde tengas tu MappedCatalogAttribute y MappedCatalogAttribute

// ── RANGO 100 al 150: tipos de Contactabilidad ──

namespace SDH.Domain.Enums
{
    /// <summary>
    /// Enum unificado que representa los tipos de contactabilidad (Teléfonos, Correos, Direcciones, etc.)
    /// </summary>
    [MappedCatalog(CatalogGroups.ContactType)]
    public enum EnumContactabilityType
    {
        [MappedCatalog("CEL")]
        Phone = 101,

        [MappedCatalog("EMAIL")]
        Email = 102,

        [MappedCatalog("TEL_C")]
        Conventional = 103,

        [MappedCatalog("WAPP")]
        WhatsApp = 104,

        [MappedCatalog("DIR_D")]
        HomeAddress = 105,

        [MappedCatalog("DIR_T")]
        WorkAddress = 106,

        [MappedCatalog("REF_P")]
        PersonalReference = 107,

        [MappedCatalog("REF_C")]
        CommercialReference = 108,

        [MappedCatalog("EMERG")]
        Emergency = 109,

        [MappedCatalog("LINK")]
        SocialNetwork = 110
    }
}