using System.Security.Cryptography;
using System.Text;

namespace SDH.infrastructure.Persistence.Services
{
    public static class ConfigCrypto
    {
        public const string EncryptedPrefix = "ENC:";

        public static bool IsEncrypted(string? value)
        {
            return !string.IsNullOrWhiteSpace(value)
                && value.StartsWith(EncryptedPrefix, StringComparison.Ordinal);
        }

        public static string Decrypt(string value, string masterKey)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            ArgumentException.ThrowIfNullOrWhiteSpace(masterKey);

            if (!IsEncrypted(value))
            {
                return value;
            }

            string base64 = value[EncryptedPrefix.Length..];
            byte[] combined = Convert.FromBase64String(base64);
            byte[] key = SHA256.HashData(Encoding.UTF8.GetBytes(masterKey));

            using Aes aes = Aes.Create();
            aes.KeySize = 256;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key;

            int ivLength = aes.BlockSize / 8;
            if (combined.Length <= ivLength)
            {
                throw new InvalidOperationException("El valor cifrado no contiene IV y payload validos.");
            }

            byte[] iv = combined[..ivLength];
            byte[] cipherBytes = combined[ivLength..];
            aes.IV = iv;

            using ICryptoTransform decryptor = aes.CreateDecryptor();
            byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }

        public static string DecryptFromEnvironment(string value, string environmentVariableName = "CONFIG_MASTER_KEY")
        {
            if (!IsEncrypted(value))
            {
                return value;
            }

            string masterKey = Environment.GetEnvironmentVariable(environmentVariableName)
                ?? throw new InvalidOperationException(
                    $"La variable de entorno {environmentVariableName} no esta configurada.");

            return Decrypt(value, masterKey);
        }
    }
}