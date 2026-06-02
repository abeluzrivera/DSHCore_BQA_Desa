using SDH.Domain.Attributes;

namespace SDH.Domain.Enums;

// ── RANGO 1 al 50: Estados de Contactabilidad ──

[MappedCatalog(CatalogGroups.ContactabilityStatus)]
public enum EnumContactabilityStatus
{
    [MappedCatalog("VERIF")]
    Verified = 1,

    [MappedCatalog("PEND")]
    Pending = 2,

    [MappedCatalog("NORES")]
    NoResponse = 3,

    [MappedCatalog("CADU")]
    Expired = 4,

    [MappedCatalog("ERROR")]
    Error = 5,

    [MappedCatalog("PEND-LOPDP")]
    PendingLOPDP = 6,

    [MappedCatalog("APRO-LOPDP")]
    AprovalLOPDP = 7,

    [MappedCatalog("Deny-LOPDP")]
    DenyLOPDP = 8,

    // ── Email ─────────────────────────────────────────────────────────────────
    [MappedCatalog("ERR_MAIL_FMT")]
    EmailFormato = 10,
    [MappedCatalog("ERR_MAIL_DOM")]
    EmailDominio = 11,
    [MappedCatalog("ERR_MAIL_TEMP")]
    EmailTemporal = 12,
    [MappedCatalog("ERR_MAIL_DUP")]
    EmailDuplicado = 13,
    [MappedCatalog("ERR_MAIL_BOUNC")]
    EmailRebote = 14,
    [MappedCatalog("ERR_MAIL_CHAR")]
    EmailCaracteres = 15,


    // ── Teléfono ──────────────────────────────────────────────────────────────
    [MappedCatalog("ERR_TEL_FMT")]
    TelFormato = 30,
    [MappedCatalog("ERR_TEL_DIGIT")]
    TelDigitos = 31,
    [MappedCatalog("ERR_TEL_NUM")]
    TelNoNumerico = 32,
    [MappedCatalog("ERR_TEL_CODE")]
    TelCodigo = 33,
    [MappedCatalog("ERR_TEL_DUMMY")]
    TelFicticio = 34,
    [MappedCatalog("ERR_TEL_DISC")]
    TelDesconectado = 35,
    [MappedCatalog("ERR_TEL_DUP")]
    TelDuplicado = 36,

    // ── Dirección ─────────────────────────────────────────────────────────────
    [MappedCatalog("ERR_DIR_SHORT")]
    DirCorta = 50,
    [MappedCatalog("ERR_DIR_LONG")]
    DirLarga = 51,
    [MappedCatalog("ERR_DIR_INC")]
    DirIncompleta = 52,
    [MappedCatalog("ERR_DIR_POBOX")]
    DirCasillero = 53,
    [MappedCatalog("ERR_DIR_DUMMY")]
    DirGenerica = 54,
    [MappedCatalog("ERR_DIR_CHAR")]
    DirCaracteres = 55,
    [MappedCatalog("ERR_DIR_GEO")]
    DirGeoInconsistente = 56


}
