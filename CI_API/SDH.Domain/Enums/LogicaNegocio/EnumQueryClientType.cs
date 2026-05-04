namespace SDH.Domain.Enums.LogicaNegocio;

/// <summary>
/// Tipo de información adicional que puede hidratarse sobre un UserCacheModel.
/// Cada valor corresponde a un segmento de datos que se carga de forma diferida.
/// </summary>
public enum EnumQueryClientType

{
    /// <summary>Datos de contactabilidad (teléfonos, emails, direcciones).</summary>
    Contact = 1,

    /// <summary>Información financiera y contable.</summary>
    Financiero = 2,

    /// <summary>Dirección principal del cliente (Es_Principal == true).</summary>
    Address = 3,
}


