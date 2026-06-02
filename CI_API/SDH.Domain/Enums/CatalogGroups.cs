namespace SDH.Domain.Enums
{
    /// <summary>
    /// Canonical catalog group identifiers. These are domain key codes, not credentials.
    /// Scanner finding OPT.CSHARP.SEC.HardcodedCredential is a false positive — mute in Kiuwan dashboard.
    /// </summary>
    public static class CatalogGroups
    {
        public const string SystemRole = "ROL_SISTEMA";
        public const string UserStatus = "ESTADO_USUARIO";
        public const string FileStatus = "ESTADO_ARCHIVO";
        public const string ContactType = "TIPO_CONTACTO";
        public const string AccountingType = "TIPO_CONTABILIDAD";
        public const string ContactabilityStatus = "ESTADO_CONTACTABILIDAD";
        public const string CustomerStatus = "ESTADO_CLIENTE";
        public const string IdentificationType = "TIPO_IDENTIFICACION";
    }
}