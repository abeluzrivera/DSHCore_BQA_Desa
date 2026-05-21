namespace SDH.Application.Settings;

public class AuthSettings
{
    public const string SectionName = "AuthSettings";
    public string Provider { get; set; } = "Database"; // "LDAP" | "Database"
    public string UnavailableMessage { get; set; } = "Usuario no disponible.";
}

public class LdapSettings
{
    public const string SectionName = "LdapSettings";
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 389;
    public bool UseSSL { get; set; } = false;
    public string BaseDn { get; set; } = string.Empty;
    public string BindDn { get; set; } = string.Empty;
    public string BindPassword { get; set; } = string.Empty;
    public string UserSearchBase { get; set; } = string.Empty;
    public string UserSearchFilter { get; set; } = "(&(objectClass=user)(sAMAccountName={0}))";
    public string GroupAttribute { get; set; } = "memberOf";
    public Dictionary<string, string> RoleGroupMappings { get; set; } = [];
}
