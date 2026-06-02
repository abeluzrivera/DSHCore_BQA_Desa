using contactabilidad_inteligente.mcv.Helpers;
using contactabilidad_inteligente.mcv.Models.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SDH.Application.DTOs.Clients;
using SDH.Application.Ports.Services;
using SDH.Application.Services;
using SDH.Domain.Enums;
using SDH.Domain.Extensions;

namespace contactabilidad_inteligente.mcv.Pages
{
    /// <summary>
    /// Perfil de Cliente — Pantalla que muestra los datos esenciales del resultValidated
    /// Con sección Contactos: Correos, Teléfonos, Direcciones
    /// </summary>
    [Authorize]
    public class ClientesModel(IClienteQueryService _IclienteQueryService,
                               ILogger<ClientesModel> _logger,
                               CustomerCommandService clientCommandService) : PageModel
    {

        public string? Id { get; set; }

        public ClientePerfilViewModel? Cliente { get; set; }

        public async Task<IActionResult> OnGetAsync(string? id)
        {
            Id = id?.Trim();

            // Si no hay ID en query string, redirigir al dashboard
            if (string.IsNullOrWhiteSpace(Id))
            {
                _logger.LogWarning("Acceso a Clientes sin parámetro Id");
                return RedirectToPage("/Dashboard");
            }

            try
            {
                // Cargar resultValidated por identificación (incluye Contactos y Direcciones)
                CustomerDto? cliente = await _IclienteQueryService.ObtenerPorIdentificacionAsync(Id ?? string.Empty);

                if (cliente == null)
                {
                    _logger.LogWarning("Cliente no encontrado: {ClienteId}", Id);
                    return RedirectToPage("/Dashboard");
                }

                // Mapear contactos y direcciones
                List<ContactViewModel> allContacts = MapContactos(cliente.Contacts);
                List<ContactViewModel> emails = [.. allContacts.Where(c =>
                    c.TipoContacto == EnumContactabilityType.Email.GetValueCatalog()
                )];

                // 2. Teléfonos: Agrupamos los valores válidos en un array y usamos .Contains()
                string[] tiposTelefono = [
                    EnumContactabilityType.Phone.GetValueCatalog(),
                    EnumContactabilityType.Conventional.GetValueCatalog(),
                    EnumContactabilityType.WhatsApp.GetValueCatalog()
                ];

                List<ContactViewModel> phones = [.. allContacts.Where(c => tiposTelefono.Contains(c.TipoContacto))];

                List<DireccionPerfilViewModel> addresses = MapDirecciones(cliente.Addresses);

                // Mapear a ViewModel
                string initials = GenerarInicialesDelNombre(cliente.FullName);

                Cliente = new ClientePerfilViewModel(
                    Id: cliente.Id,
                    Cedula: cliente.Identification,
                    Nombre: cliente.FullName,
                    Iniciales: initials,
                    EstaVerificado: cliente.IsVerified,
                    AvatarUrl: null,
                    Emails: emails,
                    Phones: phones,
                    Addresses: addresses
                );

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar cliente con ID: {ClienteId}", Id);
                return RedirectToPage("/Dashboard");
            }
        }


        #region MapContact

        /// <summary>
        /// Mapea ContactoCliente a ContactViewModel para la UI usando el Helper centralizado
        /// </summary>
        private List<ContactViewModel> MapContactos(List<CustomerContactDto?> contactos)
        {
            if (contactos == null || contactos.Count == 0) return [];

            return [.. contactos
                // Ordenamos usando el OrderPriority del helper
                .OrderBy(c =>
                {
                    var meta = ContactabilityLabelHelper.ContactTypes.GetValueOrDefault(c.ContactMediumTypeId.GetEnumFromIntCatalog<EnumContactabilityType>() ?? EnumContactabilityType.Phone);
                    return meta?.Order ?? 99;
                })
                .Select(contacto =>
                {                // 1. Extraer Metadatos del Helper centralizado
                    var metadata = ContactabilityLabelHelper.ContactTypes.GetValueOrDefault(
                                                contacto.ContactMediumTypeId.GetEnumFromIntCatalog<EnumContactabilityType>()
                                                ?? EnumContactabilityType.Phone);

                   
                    // 2. Extraer Clases según el Estado del Helper
                    // Regla: solo Pending y Verified son reconocidos; cualquier otro ID (sub-códigos de error) → Error
                    var statusEnum = contacto.VerificationStatusId.GetEnumFromIntCatalog<EnumContactabilityStatus>();
                    var resolvedStatus = statusEnum is EnumContactabilityStatus.Pending or EnumContactabilityStatus.Verified
                        ? statusEnum.Value
                        : EnumContactabilityStatus.Error;
                    var stateMetadata = ContactabilityLabelHelper.States.GetValueOrDefault(resolvedStatus);

                    // 1. Asignamos los valores por defecto (Fallback seguro)
                    string bgClass = "tw-bg-bg-subtle";
                    string borderClass = "tw-border-border";
                    string textClass = "tw-text-text-body-muted";
                    string stateName = "unknown";

                    // ¡Faltaba el signo '=' después del paréntesis de cierre de la tupla!
                    // 2. Sobrescribimos si la metadata del Helper existe
                    if (stateMetadata != null)
                    {
                        bgClass = stateMetadata.BgClass;
                        borderClass = stateMetadata.BorderClass;
                        textClass = stateMetadata.TextClass;
                        stateName = stateMetadata.State;
                    }
                    // 3. Evaluar si es un error
                    bool isError = stateName == "error" ||
                                   (contacto.VerificationStatusId != (int)EnumContactabilityStatus.Verified &&
                                    contacto.VerificationStatusId != (int)EnumContactabilityStatus.Pending);

                    // 4. Retornar el ViewModel ensamblado 
                    return new ContactViewModel
                    {
                        Id = contacto.Id,
                        Value = contacto.ContactValue,
                        TipoContacto = metadata.Code,

                        // Configuración visual del Tipo
                        Icon = metadata.Icon,
                        IconType = metadata.IconType,
                        Label = metadata.Label,
                        ValueClass = metadata.ShouldTruncate ? "truncate" : "",

                        // Configuración visual del Estado (Tailwind CSS)
                        Verified = contacto.VerificationStatusId == (int)EnumContactabilityStatus.Verified,
                        BgClass = bgClass,
                        TextClass = textClass,
                        BorderClass = borderClass,
                        State = stateName,

                        // Errores
                        ErrorCode = isError ? contacto.VerificationStatus : string.Empty,
                        ErrorLabel = isError ? ContactabilityLabelHelper.GetErrorLabelContactability(contacto.VerificationStatus) : string.Empty
                    };
                })];
        }

        #endregion

        #region MapDirecciones

        /// <summary>
        /// Mapea DireccionCliente a DireccionPerfilViewModel para la UI usando el Helper centralizado
        /// </summary>
        private List<DireccionPerfilViewModel> MapDirecciones(IReadOnlyCollection<CustomerAddressDto> direcciones)
        {
            // Verificación de memoria O(1)
            if (direcciones == null || direcciones.Count == 0) return [];

            return [.. direcciones
                    // 1. Filtramos registros corruptos o vacíos
                    .Where(d => !string.IsNullOrWhiteSpace(d.FullAddress))

                    // 2. Ordenamos: Las principales primero (true precede a false en descending)
                    .OrderByDescending(d => d.IsPrimary)

                    // 3. Proyectamos sin casts ruidosos
                    .Select(d =>
                    {
                        // A. Resolución Segura de Enums (Integridad de Catálogos O(1))
                        // Regla: solo Pending y Verified son reconocidos; cualquier otro ID (sub-códigos de error) → Error
                        var rawEstado = d.VerificationStatusId.GetValueOrDefault()
                                         .GetEnumFromIntCatalog<EnumContactabilityStatus>();
                        var estadoEnum = rawEstado is EnumContactabilityStatus.Pending or EnumContactabilityStatus.Verified
                            ? rawEstado.Value
                            : EnumContactabilityStatus.Error;

                        // Asumiendo que d.TipoDireccion es el ID int del DTO
                        var tipoEnum = d.AddressTypeId
                                        .GetEnumFromIntCatalog<EnumContactabilityType>()
                                        ?? EnumContactabilityType.HomeAddress;

                        // B. Extracción de Metadatos desde el Helper (Usando el Enum como Key)
                        var metadata = ContactabilityLabelHelper.ContactTypes.GetValueOrDefault(tipoEnum)
                                       ?? new ContactabilityLabelHelper.ContactTypeMetadata(
                                           tipoEnum.GetValueCatalog(), "location_on", "location", "Dirección", 99, false);

                        var stateMetadata = ContactabilityLabelHelper.States.GetValueOrDefault(estadoEnum);

                        (string bgClass, string borderClass, string textClass, string stateName) = stateMetadata != null
                            ? (stateMetadata.BgClass, stateMetadata.BorderClass, stateMetadata.TextClass, stateMetadata.State)
                            : ("tw-bg-bg-subtle", "tw-border-border", "tw-text-text-body-muted", "unknown");

                        // C. Evaluación centralizada de Reglas de Negocio
                        bool isError = estadoEnum.IsError();

                        // D. Ensamblaje del Record inmutable
                        return new DireccionPerfilViewModel(
                            Id: d.Id,
                            TipoContacto: metadata.Code,
                            Value: d.FullAddress!,
                            TypeLabel: metadata.Label,
                            State: stateName,
                            BgClass: bgClass,
                            BorderClass: borderClass,
                            TextClass: textClass,
                            HasGps: d.Latitude.HasValue && d.Longitude.HasValue,
                            Lat: d.Latitude,
                            Lng: d.Longitude,
                            ErrorLabel: isError ? ContactabilityLabelHelper.GetErrorLabelContactability(d.VerificationStatus) : string.Empty
                        );
                    })];
        }

        #endregion


        private static string GenerarInicialesDelNombre(string? nombreCompleto)
        {
            if (string.IsNullOrWhiteSpace(nombreCompleto))
            {
                return "NN";
            }

            string[] palabras = nombreCompleto.Split([' '], StringSplitOptions.RemoveEmptyEntries);

            if (palabras.Length >= 2)
            {
                return (palabras[0][0].ToString() + palabras[1][0].ToString()).ToUpper();
            }

            return nombreCompleto.Length > 0 ? nombreCompleto[0].ToString().ToUpper() : "?";
        }
    }

}
