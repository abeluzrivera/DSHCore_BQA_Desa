using SDH.Domain.Ports;

namespace SDH.infrastructure.Persistence.Ports.Services
{
    /// <summary>
    /// Aquí es donde escondemos la librería BCrypt. Si mañana quieres cambiar a Argon2 o a Identity de Microsoft, 
    /// solo cambias esta clase y ni el Dominio ni la Aplicación se enteran.
    /// </summary>
    public class BcryptPasswordHasher : IPasswordHasher
    {
        public string GenerarHash(string clavePlana)
        {
            return BCrypt.Net.BCrypt.HashPassword(clavePlana);
        }

        public bool Verificar(string clavePlana, string hashAlmacenado)
        {
            return BCrypt.Net.BCrypt.Verify(clavePlana, hashAlmacenado);
        }
    }
}
