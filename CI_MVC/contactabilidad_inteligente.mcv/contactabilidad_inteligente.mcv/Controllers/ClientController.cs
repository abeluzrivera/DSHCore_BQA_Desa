using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SDH.Application.DTOs.Clients;
using SDH.Application.Ports.Services;
using SDH.Application.Services;
using SDH.Domain.Enums;
using SDH.Domain.Enums.LogicaNegocio;
using SDH.Domain.Extensions;

namespace contactabilidad_inteligente.mcv.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/clientes")]
    public class ClientController(
        ILogger<ClientController> _logger,
        CustomerCommandService clientCommandService,
        ICustomerCacheService cacheService,
        IClienteQueryService queryService) : ControllerBase
    {


        // ─── Búsqueda y listado ────────────────────────────────────────────────

        /// <summary>
        /// GET /api/clientes/search?q=texto&amp;estado=todos&amp;top=10
        /// Búsqueda fuzzy por nombre / identificación — alimenta el dropdown de sugerencias.
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<CustomerSearchResultDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> BuscarClientes(
            [FromQuery] string? q,
            [FromQuery] int top = 10,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
                return Ok(Array.Empty<object>());

            try
            {
                IReadOnlyList<CustomerSearchResultDto> results =
                    await cacheService.SearchByNameAndIdentificationAsync(q.Trim(), cancellationToken);


                var items = results.Take(Math.Clamp(top, 1, 50)).Select(r => new
                {
                    id = r.Identification,
                    numericId = r.Id,
                    nombreCompleto = r.FullName,
                    identificacion = r.Identification,
                    tipo = "person",
                    fotoUrl = (string?)null,
                    email = (string?)null,
                    verificado = r.IsVerified,
                    estado = r.IsVerified ? "verificado" : "por-verificar",
                });

                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar clientes con q={Q}", q);
                return StatusCode(500, new { success = false, message = "Error interno" });
            }
        }

        /// <summary>
        /// GET /api/clientes/{id}
        /// Retorna el detalle completo de un cliente específico por su identificación (cédula).
        /// Utilizado por el dashboard para la inyección dinámica del panel de detalle.
        /// Retorna DTO agnóstico a UI - el cliente (JS) aplica reglas de presentación.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CustomerDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ObtenerClientePorId(
            string id,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { success = false, message = "ID requerido" });

            try
            {
                CustomerDto? cliente = await queryService.ObtenerPorIdentificacionAsync(id.Trim(), cancellationToken);

                if (cliente == null)
                    return NotFound(new { success = false, message = "Cliente no encontrado" });

                // Incluimos verificationState para que el JS pinte iconos correctos en la carga inicial
                // sin contener lógica de negocio en el frontend.
                var state = await _BuildVerificationStateAsync(id.Trim(), cancellationToken);
                return Ok(new { success = true, data = cliente, verificationState = state });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalle del cliente {Id}", id);
                return StatusCode(500, new { success = false, message = "Error interno" });
            }
        }

        /// <summary>
        /// GET /api/clientes?ids=ced1,ced2&amp;estado=todos
        /// Retorna la lista completa de clientes para el panel lateral.
        /// <paramref name="ids"/> filtra por cédulas (comma-separated).
        /// Si no se proporcionan IDs se devuelve lista vacía.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ObtenerClientes(
            [FromQuery] string? ids,
            [FromQuery] string? estado,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(ids))
                return Ok(new
                {
                    items = Array.Empty<object>(),
                    total = 0,
                    count = 0,
                    totalFiltrado = 0,
                    porVerificar = 0,
                    verificados = 0
                });

            try
            {
                string[] cedulas = [.. ids
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Take(500)];

                var items = new List<CustomerListItemDto>();

                foreach (string cedula in cedulas)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    CustomerDto? dto = await queryService.ObtenerPorIdentificacionAsync(cedula, cancellationToken);
                    if (dto is null) continue;

                    CustomerListItemDto item = _MapToListItem(dto);

                    if (estado == "verificados" && item.VerificationStatus != "verificado") continue;
                    if (estado == "por-verificar" && item.VerificationStatus == "verificado") continue;

                    items.Add(item);
                }

                int total = items.Count;
                int verificados = items.Count(c => c.VerificationStatus == "verificado");
                int porVerif = total - verificados;

                return Ok(new
                {
                    items,
                    total,
                    count = total,
                    totalFiltrado = total,
                    porVerificar = porVerif,
                    verificados,
                });
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499, new { success = false, message = "Solicitud cancelada" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener lista de clientes: ids={Ids}", ids);
                return StatusCode(500, new { success = false, message = "Error interno" });
            }
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private static CustomerListItemDto _MapToListItem(CustomerDto dto)
        {
            List<int> phones = [.. dto.Contacts
                .Where(c => !c.IsDeleted && c.ContactMediumTypeId is (int)EnumContactabilityType.Phone or (int)EnumContactabilityType.Conventional or (int)EnumContactabilityType.WhatsApp)
                .Select(c => c.VerificationStatusId)];

            List<int> emails = [.. dto.Contacts
                .Where(c => !c.IsDeleted && c.ContactMediumTypeId == (int)EnumContactabilityType.Email)
                .Select(c => c.VerificationStatusId)];

            List<int> dirs = [.. dto.Addresses
                .Where(d => !string.IsNullOrEmpty(d.FullAddress))
                .Select(d => d.VerificationStatusId ?? (int)EnumContactabilityStatus.Pending)];

            return new CustomerListItemDto
            (
                dto.Id,
                dto.FullName ?? dto.Identification,
                dto.Identification,
                dto.IsVerified ? "verificado" : "por-verificar",
                dto.IsVerified,
                "person",
                null,
                phones.Count != 0,
                emails.Count != 0,
                dirs.Count != 0,
                _AgregarEstadoGrupo(phones),
                _AgregarEstadoGrupo(emails),
                _AgregarEstadoGrupo(dirs)
            );
        }

        private static string _AgregarEstadoGrupo(IReadOnlyList<int> estados)
        {
            if (!estados.Any()) return "pending";
            if (estados.Any(e => e == (int)EnumContactabilityStatus.Error)) return "error";
            if (estados.All(e => e == (int)EnumContactabilityStatus.Verified)) return "verified";
            return "pending";
        }



        // ─── Verificación de contactos ─────────────────────────────────────────

        [HttpPost("{idCliente}/contacts/{idContacto}/verify")]
        public async Task<IActionResult> VerificarContacto(long idCliente, long idContacto, CancellationToken cancellationToken)
        {
            try
            {
                var result = await clientCommandService.VerifyCustomerContactability(EnumQueryClientType.Contact, idCliente, idContacto, cancellationToken);

                if (!result.IsSuccess)
                    return BadRequest(new { success = false, message = result.ErrorMessage });

                var state = await _BuildVerificationStateAsync(result.Value!, cancellationToken);
                return Ok(new { success = true, message = "Contacto verificado correctamente", verificationState = state });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar contacto {Id}", idContacto);
                return StatusCode(500, new { success = false, message = "Error interno al procesar la verificación" });
            }
        }

        [HttpPost("{idCliente}/contacts/{idContacto}/unverify/{errorCode}")]
        public async Task<IActionResult> UnverifyContactability(long idCliente, long idContacto, string? errorCode, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(errorCode)) return BadRequest("El código de error es requerido");

                var result = await clientCommandService.UnverifyCustomerContactability(
                    EnumQueryClientType.Contact, idCliente, idContacto,
                    (EnumContactabilityStatus)errorCode.GetEnumFromStringCatalog<EnumContactabilityStatus>(),
                    cancellationToken);

                if (!result.IsSuccess)
                    return BadRequest(new { success = false, message = result.ErrorMessage });

                var state = await _BuildVerificationStateAsync(result.Value!, cancellationToken);
                return Ok(new { success = true, verificationState = state });
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Error al rechazar contacto {Id}", idContacto);
                return StatusCode(500, new { success = false, message = "Error interno al procesar la verificación" });
            }
        }

        // --- SECCIÓN: DIRECCIONES ---

        [HttpPost("{idClient}/address/{idAddress}/verify")]
        public async Task<IActionResult> VerificarDireccion(long idClient, long idAddress, CancellationToken cancellationToken)
        {
            try
            {
                var result = await clientCommandService.VerifyCustomerContactability(EnumQueryClientType.Address, idClient, idAddress, cancellationToken);

                if (!result.IsSuccess)
                    return BadRequest(new { success = false, message = result.ErrorMessage });

                var state = await _BuildVerificationStateAsync(result.Value!, cancellationToken);
                return Ok(new { success = true, message = "Dirección verificada correctamente", verificationState = state });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar Direccion {Id}", idAddress);
                return StatusCode(500, new { success = false, message = "Error interno al procesar la verificación" });
            }
        }

        [HttpPost("{idCliente}/address/{idAddress}/unverify/{errorCode}")]
        public async Task<IActionResult> UnverifyAddress(long idCliente, long idAddress, string? errorCode, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(errorCode)) return BadRequest("El código de error es requerido");

                var result = await clientCommandService.UnverifyCustomerContactability(
                    EnumQueryClientType.Address, idCliente, idAddress,
                    (EnumContactabilityStatus)errorCode.GetEnumFromStringCatalog<EnumContactabilityStatus>(),
                    cancellationToken);

                if (!result.IsSuccess)
                    return BadRequest(new { success = false, message = result.ErrorMessage });

                var state = await _BuildVerificationStateAsync(result.Value!, cancellationToken);
                return Ok(new { success = true, verificationState = state });
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Error al rechazar dirección {Id}", idAddress);
                return StatusCode(500, new { success = false, message = "Error interno al procesar la verificación" });
            }
        }

        [HttpPost("{idClient}/address/{idAddress}/update/GPS")]
        public async Task<IActionResult> UpdateGPS(long idClient, long idAddress, [FromBody] SaveGpsRequest gpsDto)
        {
            try
            {
                _logger.LogInformation("Iniciando actualización de GPS para la dirección {IdAddress} del cliente {IdClient}", idAddress, idClient);
                var result = await clientCommandService.UpdateGpsAsync(idClient, idAddress, gpsDto.Latitude, gpsDto.Longitude);
                return result.IsSuccess
                    ? Ok(new { success = true, message = "GPS actualizado correctamente" })
                    : BadRequest(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar GPS de la direccion {Id}", idAddress);
                return StatusCode(500, "Error interno al procesar la actualización de GPS");
            }
        }

        // Cambiamos la ruta a una convención puramente RESTful
        [HttpPost("{idClient}/contacts")]
        public async Task<IActionResult> AddContact(long idClient, [FromBody] AddContactRequest request, CancellationToken cancellationToken)
        {
            // 1. VALIDACIÓN BÁSICA (Fail Fast)
            if (string.IsNullOrWhiteSpace(request.ContactValue))
            {
                return BadRequest(new { success = false, message = "El valor del contacto es obligatorio." });
            }

            var allowedContactTypes = new[]
            {
                EnumContactabilityType.Phone,
                EnumContactabilityType.Email,
                EnumContactabilityType.Conventional,
                EnumContactabilityType.WhatsApp
            };

            if (!allowedContactTypes.Contains(request.ContactType))
            {
                _logger.LogWarning("Intento de agregar un tipo inválido ({Tipo}) en endpoint de contactos. Cliente: {Id}", request.ContactType, idClient);
                return BadRequest(new { success = false, message = $"El tipo de contacto seleccionado no es válido para esta operación." });
            }

            try
            {
                // 3. LLAMADA AL SERVICIO (Ya sabemos que es seguro enviar null para direcciones)
                var result = await clientCommandService.AddContactabilityClient(
                    idClient,
                    request.ContactType,
                    request.ContactValue,
                    null, // Seguro, porque ya bloqueamos los Enums de direcciones
                    cancellationToken);

                return result.IsSuccess
                    ? Ok(new { success = true, message = "Contacto agregado correctamente" }) // Podría ser 201 Created también
                    : BadRequest(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al agregar contacto al cliente {Id}", idClient);

                // 4. RESPUESTA ESTANDARIZADA
                // Devolvemos JSON para que Axios no falle al intentar parsearlo
                return StatusCode(500, new { success = false, message = "Error interno del servidor al procesar la solicitud." });
            }
        }

        [HttpPost("{idClient}/addresses")]
        public async Task<IActionResult> AddAddress(long idClient, [FromBody] AddAddressRequest request, CancellationToken cancellationToken)
        {
            // 1. VALIDACIÓN DEL ENUM (Fail Fast - Solo Direcciones)
            var allowedAddressTypes = new[]
            {
                EnumContactabilityType.HomeAddress,
                EnumContactabilityType.WorkAddress
            };

            if (!allowedAddressTypes.Contains(request.AddressType))
            {
                _logger.LogWarning("Intento de agregar un tipo inválido ({Tipo}) en endpoint de direcciones. Cliente: {Id}", request.AddressType, idClient);
                return BadRequest(new { success = false, message = "El tipo de contacto debe ser una Dirección válida (Domicilio o Trabajo)." });
            }

            // 2. VALIDACIÓN DEL PAYLOAD BÁSICO
            if (string.IsNullOrWhiteSpace(request.FullAddress))
            {
                return BadRequest(new { success = false, message = "La dirección completa es obligatoria." });
            }

            try
            {
                // 3. LLAMADA AL SERVICIO
                // Usamos la dirección completa como el 'valorContacto' genérico del método, 
                // y le pasamos el objeto AddressDetails completo para que el switch lo procese.
                var result = await clientCommandService.AddContactabilityClient(
                    idClient,
                    request.AddressType,
                    string.Empty,
                    request,
                    cancellationToken);

                return result.IsSuccess
                    ? Ok(new { success = true, message = "Dirección agregada correctamente" })
                    : BadRequest(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al agregar dirección al cliente {Id}", idClient);

                // 4. RESPUESTA ESTANDARIZADA 500
                return StatusCode(500, new { success = false, message = "Error interno del servidor al procesar la dirección." });
            }
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Calcula el estado de verificación del cliente tras una operación de escritura.
        /// Regla isClientVerified: cada grupo con datos debe tener ≥1 verificado; los grupos vacíos se ignoran.
        /// GroupState: worst-wins dentro del grupo (error > pending > verified).
        /// Toda la lógica vive aquí para que el JS sea solo presentación.
        /// </summary>
        private async Task<ContactVerificationStateDto> _BuildVerificationStateAsync(
            string identification, CancellationToken ct)
        {
            var contacts = await queryService.ObtenerContactosAsync(identification, ct);
            var addresses = await queryService.ObtenerDireccionesAsync(identification, ct);

            // Clasificación de tipos por grupo
            int[] phoneTypeIds =
            [
                (int)EnumContactabilityType.Phone,
                (int)EnumContactabilityType.Conventional,
                (int)EnumContactabilityType.WhatsApp,
            ];

            var phoneIds = contacts.Where(c => !c.IsDeleted && phoneTypeIds.Contains(c.ContactMediumTypeId))
                                     .Select(c => c.VerificationStatusId).ToList();
            var emailIds = contacts.Where(c => !c.IsDeleted && c.ContactMediumTypeId == (int)EnumContactabilityType.Email)
                                     .Select(c => c.VerificationStatusId).ToList();
            var addressIds = addresses.Select(a => a.VerificationStatusId ?? (int)EnumContactabilityStatus.Pending).ToList();

            string phoneState = _GroupState(phoneIds);
            string emailState = _GroupState(emailIds);
            string addressState = _GroupState(addressIds);

            // Un grupo vacío se ignora; cada grupo con datos debe tener ≥1 verificado,
            // y al menos un grupo debe existir para considerar al cliente verificado.
            bool isClientVerified =
                (phoneIds.Count == 0 || phoneIds.Any(s => s == (int)EnumContactabilityStatus.Verified)) &&
                (emailIds.Count == 0 || emailIds.Any(s => s == (int)EnumContactabilityStatus.Verified)) &&
                (addressIds.Count == 0 || addressIds.Any(s => s == (int)EnumContactabilityStatus.Verified)) &&
                (phoneIds.Count + emailIds.Count + addressIds.Count > 0);

            return new ContactVerificationStateDto(isClientVerified, phoneState, emailState, addressState);
        }

        /// <summary>
        /// Worst-wins dentro de un grupo de contactos.
        /// Un statusId es "error" si es el código genérico (5) o cualquier ERR_* (≥10).
        /// </summary>
        private static string _GroupState(List<int> statusIds)
        {
            if (statusIds.Count == 0) return "none";
            if (statusIds.Any(s => s == (int)EnumContactabilityStatus.Verified)) return "verified";
            if (statusIds.Any(s => s == (int)EnumContactabilityStatus.Error || s >= 10)) return "error";
            return "pending";
        }
    }
}
