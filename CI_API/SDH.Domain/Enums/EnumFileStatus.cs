using SDH.Domain.Attributes;

namespace SDH.Domain.Enums
{
    [MappedCatalog(CatalogGroups.FileStatus)]
    public enum EnumFileStatus
    {
        [MappedCatalog("PEND")]
        Pending = 301,

        [MappedCatalog("VAL")]
        Validating = 302,

        [MappedCatalog("PROC")]
        Processing = 303,

        [MappedCatalog("OK")]
        Completed = 304,

        [MappedCatalog("PARC")]
        Partial = 305,

        [MappedCatalog("ERR")]
        Failed = 306,

        [MappedCatalog("CANC")]
        Cancelled = 307
    }
}
