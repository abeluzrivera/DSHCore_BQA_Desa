namespace SDH.Domain.Ports
{
    public interface IPasswordHasher
    {
        string GenerarHash(string clavePlana);
        bool Verificar(string clavePlana, string hashAlmacenado);
    }
}
