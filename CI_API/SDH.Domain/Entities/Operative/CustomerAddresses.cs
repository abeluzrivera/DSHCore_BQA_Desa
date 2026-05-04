using SDH.Domain.Contracts;
using SDH.Domain.Enums;
using SDH.Domain.Extensions;
using System.ComponentModel.DataAnnotations.Schema;

namespace SDH.Domain.Entities.Operative
{
    /// <summary>
    /// Entidad Hija: Representa una dirección geográfica registrada para un cliente.
    /// </summary>
    public class CustomerAddresses : IAuditableEntity, IVerificableEntity
    {
        public int Id { get; private set; }
        public long ClientId { get; private set; }

        public int? AddressTypeId { get; private set; }

        [NotMapped]
        public EnumContactabilityType AddressType =>
                AddressTypeId.GetValueOrDefault().GetEnumFromIntCatalog<EnumContactabilityType>()
                ?? EnumContactabilityType.HomeAddress;

        public string? FullAddress { get; private set; }
        public string? City { get; private set; }
        public string? Province { get; private set; }
        public string? PostalCode { get; private set; }
        public string? Country { get; private set; }
        public string? CountryCode { get; private set; }
        public string? CityCode { get; private set; }
        public string? ProvinceCode { get; private set; }
        public string? ParishCode { get; private set; }
        public string? Parish { get; private set; }
        public decimal? Latitude { get; private set; }
        public decimal? Longitude { get; private set; }
        public bool? IsPrimary { get; private set; }
        public bool IsDeleted { get; private set; }
        public string? Source { get; private set; }
        public int? VerificationStatusId { get; private set; }

        [NotMapped]
        public EnumContactabilityStatus VerificationStatus =>
                VerificationStatusId.GetValueOrDefault().GetEnumFromIntCatalog<EnumContactabilityStatus>()
                ?? EnumContactabilityStatus.Pending;

        // Auditoría
        public DateTime? CreatedAt { get; private set; }
        public string? CreatedBy { get; private set; }
        public DateTime? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }

        public string? VerifiedBy { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public string? ApprovedBy { get; private set; }
        public DateTime? ApprovedAt { get; private set; }

        public string? LopdpStatus { get; private set; } = string.Empty;

        // Constructor privado para EF Core
        private CustomerAddresses() { }

        /// <summary>
        /// Factory method para registrar una nueva dirección de cliente.
        /// </summary>
        public static CustomerAddresses Create(
            int? tipoDireccion,
            string? direccionCompleta,
            string? ciudad,
            string? provincia,
            string? codigoPostal,
            string? pais,
            string? codigoPais,
            string? codigoCiudad,
            string? codigoProvincia,
            decimal? latitud,
            decimal? longitud,
            bool esPrincipal,
            string? usuarioCreacion)
        {
            return new CustomerAddresses
            {
                AddressTypeId = tipoDireccion,
                FullAddress = direccionCompleta,
                City = ciudad,
                Province = provincia,
                PostalCode = codigoPostal,
                Country = pais,
                CountryCode = codigoPais,
                CityCode = codigoCiudad,
                ProvinceCode = codigoProvincia,
                Latitude = latitud,
                Longitude = longitud,
                IsPrimary = esPrincipal,
                IsDeleted = false,
                Source = GlobalVariables.SystemUser,
                VerificationStatusId = (int)EnumContactabilityStatus.Pending,
                CreatedAt = DateTime.Now,
                CreatedBy = usuarioCreacion,
            };
        }

        public void Update(
            string? direccionCompleta,
            string? ciudad,
            string? provincia,
            string? codigoPostal,
            decimal? latitud,
            decimal? longitud,
            string? usuarioActualiza)
        {
            FullAddress = direccionCompleta;
            City = ciudad;
            Province = provincia;
            PostalCode = codigoPostal;
            Latitude = latitud;
            Longitude = longitud;
            LastModifiedBy = usuarioActualiza;
            LastModifiedAt = DateTime.Now; // Añadido para mantener la consistencia en la auditoría
        }

        public void UpdateGPS(decimal? latitud, decimal? longitud)
        {
            Latitude = latitud;
            Longitude = longitud;
        }

        public void Verify(string idUsuarioVerificador)
        {
            VerifiedBy = idUsuarioVerificador;
            VerifiedAt = DateTime.Now;
            VerificationStatusId = (int)EnumContactabilityStatus.Verified;
        }

        public void UnVerify(string idUsuarioVerificador, EnumContactabilityStatus error)
        {
            VerifiedBy = idUsuarioVerificador;
            VerifiedAt = DateTime.Now;
            VerificationStatusId = (int)error;
        }
    }
}
