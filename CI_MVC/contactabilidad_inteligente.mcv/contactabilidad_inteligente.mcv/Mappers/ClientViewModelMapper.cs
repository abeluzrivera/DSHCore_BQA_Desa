using contactabilidad_inteligente.mcv.Helpers;
using contactabilidad_inteligente.mcv.Models.Client;
using SDH.Application.DTOs.Clients;
using SDH.Domain.Enums;
using SDH.Domain.Extensions;

namespace contactabilidad_inteligente.mcv.Mappers
{
    /// <summary>
    /// Mapper centralizado para convertir DTOs a ViewModels.
    /// Usado por las páginas Razor (Server-Side Rendering).
    /// NO usar en Controllers API - los Controllers deben retornar DTOs puros.
    /// </summary>
    public static class ClientViewModelMapper
    {
        public static ClientViewModel MapToViewModel(CustomerDto cliente)
        {
            List<ContactViewModel> contacts = MapContactos(cliente.Contacts);
            contacts.AddRange(MapDirecciones(cliente.Addresses));

            return new ClientViewModel
            {
                NumericId = cliente.Id,
                Id = cliente.Identification,
                Name = cliente.FullName ?? cliente.Identification,
                Initials = GetInitials(cliente.FullName ?? cliente.Identification),
                AvatarUrl = null,
                Status = cliente.IsVerified ? "Verificado" : "Pendiente",
                StatusClass = cliente.IsVerified
                                   ? "bg-green-100 text-green-800"
                                   : "bg-orange-50 text-alert-ochre",
                Location = null,
                Contacts = contacts
            };
        }

        public static List<ContactViewModel> MapContactos(List<CustomerContactDto> contactos)
        {
            return [.. contactos
                // El Where es excelente, rápido y directo a nivel numérico
                .Where(c => !c.IsDeleted && c.VerificationStatusId != (int)EnumContactabilityStatus.Error)
                .OrderBy(c => c.ContactMediumTypeId)
                .Select(c =>
                {
                    // 1. Obtener los Enums UNA SOLA VEZ de forma segura (Fallback a Pending si hay basura en BD)
                    var estadoEnum = c.VerificationStatusId.GetEnumFromIntCatalog<EnumContactabilityStatus>()
                                     ?? EnumContactabilityStatus.Pending;

                    var tipoContactoEnum = c.ContactMediumTypeId.GetEnumFromIntCatalog<EnumContactabilityType>();

                    // 2. Extraer representaciones de texto sin riesgo de NullReferenceException
                    string estadoTexto = estadoEnum.GetValueCatalog();
                    string tipoContactoTexto = tipoContactoEnum?.GetValueCatalog() ?? string.Empty;

                    // 3. Evaluar reglas de negocio usando el método de extensión tipado
                    bool isError = estadoEnum.IsError(); 

                    // 4. Mapeo ultra rápido en memoria
                    return new ContactViewModel
                    {
                        Id = c.Id,
                        Icon = GetContactIcon(c.ContactMediumTypeId),
                        Label = GetContactLabel(c.ContactMediumTypeId),
                        Value = c.ContactValue,
                        TipoContacto = tipoContactoTexto,
                        // Comparación nativa de Enums en lugar de llamadas a métodos
                        Verified = estadoEnum == EnumContactabilityStatus.Verified,
                        BgClass = GetContactBgClass(c.VerificationStatusId),
                        BorderClass = GetContactBorderClass(c.VerificationStatusId),
                        ValueClass = ShouldTruncate(c.ContactMediumTypeId) ? "truncate" : "",
                        State = GetContactState(c.VerificationStatusId),
                        IconType = GetContactIconType(c.ContactMediumTypeId),
                        ErrorCode = isError ? estadoTexto : string.Empty,
                        ErrorLabel = isError ? ContactabilityLabelHelper.GetErrorLabelContactability(estadoTexto) : string.Empty
                    };
                })];
        }

        public static List<ContactViewModel> MapDirecciones(List<CustomerAddressDto> direcciones)
        {
            return [.. direcciones
                .Where(d => !string.IsNullOrWhiteSpace(d.FullAddress))
                // Simplificación: Si EsPrincipal es bool?, evaluamos directamente
                .OrderBy(d => d.IsPrimary == true ? 0 : 1)
                .Select(d => // Eliminado el cast ruidoso (Func<...>)
                {
                    // 1. Obtener el Enum de Estado de forma segura UNA SOLA VEZ
                    var estadoEnum = d.VerificationStatusId.GetValueOrDefault()
                                      .GetEnumFromIntCatalog<EnumContactabilityStatus>()
                                      ?? EnumContactabilityStatus.Pending;

                    // 2. Extraer representaciones (int y string) a partir del estado seguro
                    int estadoVerificacion = (int)estadoEnum;
                    string estadoTexto = estadoEnum.GetValueCatalog();

                    // 3. Evaluar reglas de negocio usando el Enum directamente (Más rápido)
                    bool isError = estadoEnum.IsError(); // Utiliza el método de extensión tipado que creamos

                    // 4. Mapeo seguro del Tipo de Contacto
                    string tipoContactoTexto = d.AddressTypeId
                                                .GetEnumFromIntCatalog<EnumContactabilityType>()?
                                                .GetValueCatalog()
                                                ?? EnumContactabilityType.HomeAddress.GetValueCatalog();

                    return new ContactViewModel
                    {
                        Id = d.Id,
                        Icon = GetContactIcon(d.AddressTypeId),
                        Label = GetContactLabel(d.AddressTypeId),
                        Value = d.FullAddress!,
                        TipoContacto = tipoContactoTexto,
                        // 5. Comparación directa entre Enums (Mucho más eficiente que comparar strings)
                        Verified = estadoEnum == EnumContactabilityStatus.Verified,
                        BgClass = GetContactBgClass(estadoVerificacion),
                        BorderClass = GetContactBorderClass(estadoVerificacion),
                        ValueClass = "truncate",
                        State = GetContactState(estadoVerificacion),
                        IconType = "location",
                        ErrorCode = isError ? estadoTexto : string.Empty,
                        ErrorLabel = isError ? ContactabilityLabelHelper.GetErrorLabelContactability(estadoTexto) : string.Empty,
                    };
                })];
        }


        public static string GetInitials(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "??";
            string[] parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2) return $"{parts[0][0]}{parts[1][0]}".ToUpper();
            if (parts.Length == 1 && parts[0].Length >= 2) return parts[0][..2].ToUpper();
            return parts[0][0].ToString().ToUpper();
        }

        private static string GetContactIcon(int tipo) => tipo switch
        {
            (int)EnumContactabilityType.Email => "mail",
            (int)EnumContactabilityType.Conventional => "phone",
            (int)EnumContactabilityType.Phone => "smartphone",
            (int)EnumContactabilityType.WorkAddress => "business",
            (int)EnumContactabilityType.HomeAddress => "home",
            (int)EnumContactabilityType.WhatsApp => "chat",
            (int)EnumContactabilityType.SocialNetwork => "link",
            _ => "contact_page"
        };

        private static string GetContactLabel(int tipo) => tipo switch
        {
            (int)EnumContactabilityType.Email => "Email",
            (int)EnumContactabilityType.Conventional => "Teléfono",
            (int)EnumContactabilityType.Phone => "Celular",
            (int)EnumContactabilityType.WorkAddress => "Dir. Trabajo",
            (int)EnumContactabilityType.HomeAddress => "Dir. Domicilio",
            (int)EnumContactabilityType.WhatsApp => "WhatsApp",
            (int)EnumContactabilityType.SocialNetwork => "LinkedIn",
            _ => "Contacto"
        };

        private static string GetContactIconType(int tipo) => tipo switch
        {
            (int)EnumContactabilityType.Email => "email",
            (int)EnumContactabilityType.Phone or (int)EnumContactabilityType.Conventional or (int)EnumContactabilityType.WhatsApp => "phone",
            (int)EnumContactabilityType.HomeAddress or (int)EnumContactabilityType.WorkAddress => "location",
            (int)EnumContactabilityType.SocialNetwork => "link",
            _ => "phone"
        };

        private static string GetContactState(int estado) => estado switch
        {
            (int)EnumContactabilityStatus.Verified => "verified",
            (int)EnumContactabilityStatus.Pending => "pending",
            _ => "error"
        };

        private static string GetContactBgClass(int estado) =>
            EnumContactabilityStatusExtensions.IsError(estado) ? "bg-red-50" : "bg-background-light/30";

        private static string GetContactBorderClass(int estado) =>
            EnumContactabilityStatusExtensions.IsError(estado) ? "border-red-100" : "border-forest-green/5";

        private static bool ShouldTruncate(int tipo) =>
            tipo is (int)EnumContactabilityType.Email
                or (int)EnumContactabilityType.HomeAddress
                or (int)EnumContactabilityType.WorkAddress
                or (int)EnumContactabilityType.SocialNetwork;
    }
}
