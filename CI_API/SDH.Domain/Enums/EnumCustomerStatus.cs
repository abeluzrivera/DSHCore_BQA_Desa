using SDH.Domain.Attributes;

namespace SDH.Domain.Enums
{
    [MappedCatalog(CatalogGroups.CustomerStatus)]
    public enum EnumCustomerStatus
    {
        [MappedCatalog("PROS")]
        Prospect = 351,

        [MappedCatalog("PEND")]
        Pending = 352,

        [MappedCatalog("VERIF")]
        Verified = 353,

        [MappedCatalog("INACT")]
        Inactive = 354,

        [MappedCatalog("RECH")]
        Rejected = 355,

        [MappedCatalog("BLOQ")]
        Blocked = 356
    }
}
