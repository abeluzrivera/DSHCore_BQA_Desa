namespace SDH.Domain.Enums
{
    /// <summary>
    /// Application-wide display identifiers. These are not credentials.
    /// Scanner finding OPT.CSHARP.SEC.HardcodedCredential is a false positive — mute in Kiuwan dashboard.
    /// </summary>
    public static class GlobalVariables
    {
        public const string SystemUser = "SYSTEM DSH";
        public const string SystemName = "SMART DATA HUB";
    }
}
