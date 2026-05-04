using System.Security.Cryptography;
using System.Text;

namespace SDH.SecretTool
{
    internal static class AesEncryptor
    {
        private const string EncryptedPrefix = "ENC:";
        private const string MasterKeyEnvironmentVariable = "CONFIG_MASTER_KEY";
        private const string AlphaNumericChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private const string SymbolChars = "!@#$%^&*_-+=";

        public static string Encrypt(string plainText, string masterKey)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(plainText);
            ArgumentException.ThrowIfNullOrWhiteSpace(masterKey);

            byte[] key = SHA256.HashData(Encoding.UTF8.GetBytes(masterKey));
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

            using Aes aes = Aes.Create();
            aes.KeySize = 256;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key;
            aes.GenerateIV();

            using ICryptoTransform encryptor = aes.CreateEncryptor();
            byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            byte[] combined = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, combined, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, combined, aes.IV.Length, cipherBytes.Length);

            return EncryptedPrefix + Convert.ToBase64String(combined);
        }

        public static string Decrypt(string encryptedValue, string masterKey)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(encryptedValue);
            ArgumentException.ThrowIfNullOrWhiteSpace(masterKey);

            if (!IsEncrypted(encryptedValue))
            {
                return encryptedValue;
            }

            string base64 = encryptedValue[EncryptedPrefix.Length..];
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
                throw new InvalidOperationException("El valor cifrado no es valido.");
            }

            byte[] iv = combined[..ivLength];
            byte[] cipherBytes = combined[ivLength..];
            aes.IV = iv;

            using ICryptoTransform decryptor = aes.CreateDecryptor();
            byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }

        public static bool IsEncrypted(string value)
        {
            return !string.IsNullOrWhiteSpace(value)
                && value.StartsWith(EncryptedPrefix, StringComparison.Ordinal);
        }

        public static bool TryGetMasterKey(out string masterKey, out string errorMessage)
        {
            masterKey = Environment.GetEnvironmentVariable(MasterKeyEnvironmentVariable, EnvironmentVariableTarget.Machine) ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(masterKey))
            {
                errorMessage = string.Empty;
                return true;
            }

            errorMessage =
                $"La variable de entorno {MasterKeyEnvironmentVariable} es obligatoria para cifrar o descifrar valores.";
            return false;
        }

        public static string GenerateSecureKey(int length, bool includeSymbols)
        {
            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "La longitud debe ser mayor que cero.");
            }

            string chars = includeSymbols
                ? AlphaNumericChars + SymbolChars
                : AlphaNumericChars;

            char[] buffer = new char[length];
            for (int i = 0; i < buffer.Length; i++)
            {
                int index = RandomNumberGenerator.GetInt32(chars.Length);
                buffer[i] = chars[index];
            }

            return new string(buffer);
        }
    }
}