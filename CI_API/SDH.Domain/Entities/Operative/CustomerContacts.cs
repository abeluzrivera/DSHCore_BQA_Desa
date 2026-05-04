using SDH.Domain.Contracts;
using SDH.Domain.Enums;
using SDH.Domain.Extensions;
using System.ComponentModel.DataAnnotations.Schema;

namespace SDH.Domain.Entities.Operative
{
    public class CustomerContacts : IAuditableEntity
    {
        public int Id { get; private set; }
        public long ClientId { get; private set; }

        public int ContactMediumTypeId { get; private set; }

        [NotMapped]
        public EnumContactabilityType ContactMediumType =>
            ContactMediumTypeId.GetEnumFromIntCatalog<EnumContactabilityType>()
            ?? EnumContactabilityType.Phone;


        public string ContactValue { get; private set; } = string.Empty;
        public int VerifyStatusId { get; private set; }

        [NotMapped]
        public EnumContactabilityStatus VerifyStatus =>
            VerifyStatusId.GetEnumFromIntCatalog<EnumContactabilityStatus>()
            ?? EnumContactabilityStatus.Pending;

        public string? VerifiedBy { get; private set; }

        public DateTime? VerifiedAt { get; set; }
        public DateTime CreatedAt { get; private set; }
        public string CreatedBy { get; private set; } = string.Empty;


        public bool IsDeleted { get; private set; }
        public string Source { get; private set; } = GlobalVariables.SystemName;
        public int LOPDPStatusId { get; private set; }

        [NotMapped]
        public EnumContactabilityStatus LOPDPStatus =>
            LOPDPStatusId.GetEnumFromIntCatalog<EnumContactabilityStatus>()
            ?? EnumContactabilityStatus.Pending;

        public string? LastModifiedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }

        // Constructor privado requerido para que los ORMs (EF Core) puedan instanciar la clase al leer de BD
        private CustomerContacts()
        {

        }

        // Factory method (Se elimina IdCliente de los parámetros porque el Aggregate Root lo gestiona)
        public static CustomerContacts Create(
            int tipoMedioContacto,
            string valorContacto,
            string usuarioCreacion,
            string source = GlobalVariables.SystemName)
        {
            return new CustomerContacts
            {
                ContactMediumTypeId = tipoMedioContacto,
                ContactValue = valorContacto,
                VerifyStatusId = (int)EnumContactabilityStatus.Pending,
                CreatedAt = DateTime.Now,
                CreatedBy = usuarioCreacion,
                IsDeleted = false,
                Source = source,
                LOPDPStatusId = (int)EnumContactabilityStatus.Pending
            };
        }

        public void Verify(string idUsuarioVerificador)
        {
            VerifiedBy = idUsuarioVerificador;
            VerifiedAt = DateTime.Now;
            VerifyStatusId = (int)EnumContactabilityStatus.Verified;
        }

        public void UnVerify(string idUsuarioVerificador, EnumContactabilityStatus error)
        {
            VerifiedBy = idUsuarioVerificador;
            VerifiedAt = DateTime.Now;
            VerifyStatusId = (int)error;
        }

        public void MarkAsDeleted(string usuarioModificacion)
        {
            IsDeleted = true;
        }
    }
}
