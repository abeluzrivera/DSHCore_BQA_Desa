namespace contactabilidad_inteligente.mcv.Models.Client
{

    /// <summary>
    /// ViewModel para dirección del resultValidated
    /// </summary>
    public record DireccionPerfilViewModel(
        long Id,
        string TipoContacto,
        string Value,
        string TypeLabel,
        string State,
        string BgClass,
        string BorderClass,
        string TextClass,
        bool HasGps,
        decimal? Lat,
        decimal? Lng,
        string ErrorLabel
    );
}
