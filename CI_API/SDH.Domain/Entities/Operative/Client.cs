using SDH.Domain.Contracts;
using SDH.Domain.Enums;
using SDH.Domain.Enums.LogicaNegocio;

namespace SDH.Domain.Entities.Operative
{
    /// <summary>
    /// Aggregate Root: Representa un cliente del sistema con información básica y auditoría.
    /// </summary>
    public class Client : IAuditableEntity
    {
        public long Id { get; private set; } // Renombrado a Id por convención
        public string Identification { get; private set; } = string.Empty;
        public int IdentificationType { get; private set; }
        public string? FullName { get; private set; }
        public bool IsVerified { get; private set; }
        public bool IsApproved { get; private set; }
        public bool IsDeleted { get; private set; }
        public bool IsAnonymized { get; private set; }

        public DateTime CreatedAt { get; private set; }
        public string CreatedBy { get; private set; } = string.Empty;
        public DateTime? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime? LegalExpirationDate { get; private set; }


        // Navigation properties - Relaciones 1:N
        private readonly List<ClientFinancial> _financials = [];
        private readonly List<CustomerContacts> _contacts = [];
        private readonly List<CoreOfficializations> _officializations = [];
        private readonly List<CustomerAddresses> _addresses = [];

        public IReadOnlyCollection<ClientFinancial> Financials => _financials.AsReadOnly();
        public IReadOnlyCollection<CustomerContacts> Contacts => _contacts.AsReadOnly();
        public IReadOnlyCollection<CoreOfficializations> Officializations => _officializations.AsReadOnly();
        public IReadOnlyCollection<CustomerAddresses> Addresses => _addresses.AsReadOnly();

        // Constructor privado para EF Core
        private Client() { }

        // Factory method
        public static Client Create(string identification, string? fullName, string creationUser, int identificationType = (int)EnumIdentificationType.DNI)
        {
            // Validate the identification type
            if (!Enum.IsDefined(typeof(EnumIdentificationType), identificationType))
            {
                throw new ArgumentException("Invalid identification type", nameof(identificationType));
            }

            return new Client
            {
                Identification = identification,
                IdentificationType = identificationType, // Convert enum to string if necessary
                FullName = fullName,
                CreatedAt = DateTime.Now,
                CreatedBy = creationUser,
                IsVerified = false,
                IsApproved = false,
                IsDeleted = false,
                IsAnonymized = false
            };
        }

        public void UpdateNames(string? nombreCompleto, string usuarioModificacion)
        {
            FullName = nombreCompleto;
            LastModifiedAt = DateTime.Now;
            LastModifiedBy = usuarioModificacion;
        }

        // 1. Agrega este diccionario privado en la parte superior de tu clase Cliente
        private static readonly Dictionary<int, string> ContactGroupMapping = new()
        {
            [(int)EnumContactabilityType.Phone] = "phone",
            [(int)EnumContactabilityType.Conventional] = "phone",
            [(int)EnumContactabilityType.WhatsApp] = "phone",

            [(int)EnumContactabilityType.Email] = "email",

            [(int)EnumContactabilityType.HomeAddress] = "address",
            [(int)EnumContactabilityType.WorkAddress] = "address",

            [(int)EnumContactabilityType.PersonalReference] = "reference",
            [(int)EnumContactabilityType.CommercialReference] = "reference",

            [(int)EnumContactabilityType.Emergency] = "emergency",
            [(int)EnumContactabilityType.SocialNetwork] = "social"
        };

        // 2. Tu función original se reduce a esta única línea elegante
        public static string GetGrupoContacto(int tipo)
        {
            // Busca en el diccionario. Si no encuentra el tipo, devuelve el mismo tipo como fallback (_ => tipo)
            return ContactGroupMapping.GetValueOrDefault(tipo, "unknown");
        }

        public void UpdateVerificationStatus()
        {
            var activeContacts = _contacts.Where(c => !c.IsDeleted).ToList();
            var activeAddresses = _addresses.Where(d => !d.IsDeleted).ToList();

            // Regla base: debe haber al menos un contacto O una dirección activa
            if (activeContacts.Count == 0 && activeAddresses.Count == 0)
            {
                IsVerified = false;
                return;
            }

            bool contactsOk = activeContacts.Count == 0 || VerifyContacts(activeContacts);
            bool addressesOk = activeAddresses.Count == 0 || VerifyAddresses(activeAddresses);

            IsVerified = contactsOk && addressesOk;
        }

        private bool VerifyContacts(List<CustomerContacts> activeContacts)
        {
            if (activeContacts.Count == 0) return false; // Sin contactos → no verificado

            var grouped = activeContacts.GroupBy(c => GetGrupoContacto(c.ContactMediumTypeId));
            return grouped.All(g => g.Any(c => c.VerifyStatusId == (int)EnumContactabilityStatus.Verified));
        }

        private bool VerifyAddresses(List<CustomerAddresses> activeAddresses)
        {
            if (activeAddresses.Count == 0) return false; // Sin direcciones → no verificado

            return activeAddresses.Any(d => d.VerificationStatusId == (int)EnumContactabilityStatus.Verified);
        }

        public void AddContact(CustomerContacts contacto)
        {
            _contacts.Add(contacto);
            UpdateVerificationStatus(); // Regla de negocio: al agregar, se reevalúa
        }

        public void AddAddress(CustomerAddresses direccion) => _addresses.Add(direccion);

        public void VerifyContactability(EnumQueryClientType tipoContacto, long idContacto, string usuarioSistema)
        {
            if (tipoContacto == EnumQueryClientType.Contact)
            {
                var contacto = Contacts.FirstOrDefault(c => c.Id == idContacto)
                    ?? throw new InvalidOperationException($"No se encontró el contacto con ID {idContacto} asociado a este cliente.");

                contacto.Verify(usuarioSistema);
            }
            else if (tipoContacto == EnumQueryClientType.Address)
            {
                var direccion = Addresses.FirstOrDefault(d => d.Id == idContacto)
                    ?? throw new InvalidOperationException($"No se encontró la dirección con ID {idContacto} asociada a este cliente.");

                direccion.Verify(usuarioSistema);
            }
            else
            {
                throw new ArgumentException($"El tipo de consulta '{tipoContacto}' no está soportado para aprobación.");
            }

            UpdateVerificationStatus();
        }

        public void UnverifyContactability(EnumQueryClientType tipoContacto, long idContacto, EnumContactabilityStatus errorCode, string userUnverifiy)
        {
            if (tipoContacto == EnumQueryClientType.Contact)
            {
                var contacto = Contacts.FirstOrDefault(c => c.Id == idContacto)
                    ?? throw new InvalidOperationException($"No se encontró el contacto con ID {idContacto} asociado a este cliente.");
                contacto.UnVerify(userUnverifiy, errorCode);
            }
            else if (tipoContacto == EnumQueryClientType.Address)
            {
                var direccion = Addresses.FirstOrDefault(d => d.Id == idContacto)
                    ?? throw new InvalidOperationException($"No se encontró la dirección con ID {idContacto} asociada a este cliente.");
                direccion.UnVerify(userUnverifiy, errorCode);
            }
            else
            {
                throw new ArgumentException($"El tipo de consulta '{tipoContacto}' no está soportado para rechazo.");
            }
            UpdateVerificationStatus();
        }

        public void UpdateGPS(decimal? latitud, decimal? longitud, long idAddress)
        {
            var direccion = _addresses.FirstOrDefault(d => d.Id == idAddress)
                ?? throw new InvalidOperationException($"La dirección con ID {idAddress} no pertenece a este cliente.");

            direccion.UpdateGPS(latitud, longitud);

        }
    }
}
