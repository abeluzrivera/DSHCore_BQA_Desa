using SDH.Domain.Attributes;

namespace SDH.Domain.Enums
{
    /// <summary>
    /// Roles del sistema. Mapeados al catálogo ROL_SISTEMA en base de datos.
    /// Usar .GetValueCatalog() para obtener el código string ("ADMIN", "USER", etc.)
    /// </summary>
    [MappedCatalog(CatalogGroups.SystemRole)]
    public enum EnumSystemRole
    {
        [MappedCatalog("ADMIN")]
        Administrator = 251,

        [MappedCatalog("USER")]
        Operator = 252,

        [MappedCatalog("AUDIT")]
        Auditor = 253,

        [MappedCatalog("SUPAD")]
        SuperAdmin = 254
    }
}
