namespace SDH.Application.Ports.Services
{
    /// <summary>
    /// Descifra valores de configuracion marcados con el prefijo ENC:.
    /// Los valores sin ese prefijo se devuelven sin modificacion.
    /// </summary>
    public interface IConfigDecryptionService
    {
        /// <summary>
        /// Descifra el valor si tiene prefijo ENC:, o lo devuelve tal cual.
        /// </summary>
        string Decrypt(string value);

        /// <summary>
        /// Retorna true si el valor tiene el prefijo ENC:.
        /// </summary>
        bool IsEncrypted(string value);
    }
}