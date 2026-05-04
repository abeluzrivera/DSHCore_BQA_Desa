using SDH.Domain.Attributes;

namespace SDH.Domain.Enums
{
    /// <summary>
    /// Estados de un usuario. Mapeados al catálogo ESTADO_USUARIO en base de datos.
    /// Usar .GetValueCatalog() para obtener el código string ("ACT", "INA", etc.)
    /// </summary>
    [MappedCatalog(CatalogGroups.UserStatus)]
    public enum EnumUserStatus
    {
        [MappedCatalog("ACT")]
        Active = 151,

        [MappedCatalog("INA")]
        Inactive = 152,

        [MappedCatalog("BLO")]
        Locked = 153,

        [MappedCatalog("SUSP")]
        Suspended = 154,

        [MappedCatalog("REQ_PW")]
        PasswordChangeRequired = 155
    }
}
