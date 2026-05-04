namespace SDH.Application.Ports.Services
{
    public interface ICurrentUserService
    {
        string? ObtenerUsuarioActual();
        string? ObtenerIpActual();
    }
}