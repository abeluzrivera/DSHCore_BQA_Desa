namespace contactabilidad_inteligente.mcv.Models.Client
{
    /// <summary>
    /// ViewModel para el perfil del resultValidated con contactabilidad
    /// </summary>
    public record ClientePerfilViewModel(
        long Id,
        string Cedula,
        string? Nombre,
        string Iniciales,
        bool EstaVerificado,
        string? AvatarUrl,
        List<ContactViewModel>? Emails = null,
        List<ContactViewModel>? Phones = null,
        List<DireccionPerfilViewModel>? Addresses = null
    );
}
