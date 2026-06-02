
namespace SDH.Domain.Contracts
{
    /// <summary>
    /// contrato para entidades que requieren campos de verificación, como UsuarioVerificador y FechaVerificacion.
    /// </summary>
    public interface IVerificableEntity
    {
        public string? VerifiedBy { get; set; }
        public DateTime? VerifiedAt { get; set; }
    }
}
