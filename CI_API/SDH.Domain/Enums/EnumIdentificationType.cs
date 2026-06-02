using SDH.Domain.Attributes;

namespace SDH.Domain.Enums
{
    [MappedCatalog(CatalogGroups.IdentificationType)]
    public enum EnumIdentificationType
    {
        [MappedCatalog("DNI")]
        DNI = 201,

        [MappedCatalog("RUC")]
        RUC = 202,

        [MappedCatalog("PAS")]
        PAS = 203,

        [MappedCatalog("OTR")]
        OTR = 204,
    }
}
