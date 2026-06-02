using Microsoft.Extensions.Logging;
using SDH.Application.Ports.Services;

namespace SDH.infrastructure.Persistence.Services
{
    public class ConfigDecryptionService(ILogger<ConfigDecryptionService> logger) : IConfigDecryptionService
    {
        public bool IsEncrypted(string value)
        {
            return ConfigCrypto.IsEncrypted(value);
        }

        public string Decrypt(string value)
        {
            if (!IsEncrypted(value))
            {
                return value;
            }

            try
            {
                return ConfigCrypto.DecryptFromEnvironment(value);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al descifrar valor de configuracion.");
                throw new InvalidOperationException("No se pudo descifrar el valor de configuracion.", ex);
            }
        }
    }
}