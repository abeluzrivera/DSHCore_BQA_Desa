using Microsoft.Extensions.Logging;
using SDH.Application.Common;
using SDH.Application.DTOs.Clients;
using SDH.Application.Ports.Services;
using SDH.Domain.Entities.Operative;
using SDH.Domain.Enums;
using SDH.Domain.Enums.LogicaNegocio;
using SDH.Domain.Repositories;

namespace SDH.Application.Services
{
    public class CustomerCommandService(
        IUnitOfWork unitOfWork,
        ICustomerRepository _IClienteRepository,
        IClienteQueryService ClienteQueryService,
        ICurrentUserService _currentUserService,
        ICustomerCacheService _cacheService,
        ILogger<CustomerCommandService> logger)
    {

        private readonly string _usuarioActual = _currentUserService.ObtenerUsuarioActual() ?? GlobalVariables.SystemUser;

        public async Task<Result<long>> CrearClienteAsync(
            string identificacion, string nombreCompleto, string usuarioCreacion, CancellationToken cancellationToken = default)
        {
            try
            {
                if (await _IClienteRepository.ExistsByIdentificationAsync(identificacion, cancellationToken))
                {
                    return Result<long>.Failure($"Ya existe un cliente con identificación {identificacion}");
                }

                Client cliente = Client.Create(identificacion, nombreCompleto, usuarioCreacion);

                await _IClienteRepository.AddCustomerAsync(cliente, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken); // El UoW solo confirma

                logger.LogInformation("Cliente creado exitosamente: {ClienteId}", cliente.Id);

                return Result<long>.Success(cliente.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al crear cliente con identificación {Identificacion}", identificacion);
                return Result<long>.Failure("Error al crear el cliente");
            }
        }

        public async Task<Result<long>> AddContactabilityClient(
                                                                long idCliente,
                                                                EnumContactabilityType tipoMedio,
                                                                string? valorContacto,
                                                                AddAddressRequest? addDireccionRequest,
                                                                CancellationToken cancellationToken = default)
        {
            try
            {
                var cliente = await _IClienteRepository.GetByIdAsync(idCliente, cancellationToken);
                if (cliente == null)
                {
                    return Result<long>.Failure($"No se encontró el cliente con ID {idCliente}");
                }

                int newId = 0;

                switch (tipoMedio)
                {
                    // --- GRUPO: CONTACTOS BÁSICOS ---
                    case EnumContactabilityType.Phone:
                    case EnumContactabilityType.Email:
                    case EnumContactabilityType.Conventional:
                    case EnumContactabilityType.WhatsApp:
                        {
                            var contacto = CustomerContacts.Create((int)tipoMedio, valorContacto ?? string.Empty, _usuarioActual);
                            cliente.AddContact(contacto);

                            await unitOfWork.SaveChangesAsync(cancellationToken);
                            newId = contacto.Id;
                            break;
                        }

                    // --- GRUPO: DIRECCIONES ---
                    case EnumContactabilityType.HomeAddress:
                    case EnumContactabilityType.WorkAddress:
                        {
                            // Validación estricta antes de intentar usar el objeto
                            if (addDireccionRequest == null)
                            {
                                return Result<long>.Failure("Los datos de la dirección son obligatorios para este tipo de contacto.");
                            }

                            var direccion = CustomerAddresses.Create(
                                (int)tipoMedio,
                                addDireccionRequest.FullAddress,
                                addDireccionRequest.City,
                                addDireccionRequest.Province,
                                addDireccionRequest.PostalCode,
                                addDireccionRequest.Country,
                                addDireccionRequest.CountryCode,
                                addDireccionRequest.CityCode,
                                addDireccionRequest.ProvinceCode,
                                addDireccionRequest.Latitude,
                                addDireccionRequest.Longitude,
                                addDireccionRequest.IsPrimary ?? false,
                                _usuarioActual);

                            cliente.AddAddress(direccion);

                            await unitOfWork.SaveChangesAsync(cancellationToken);
                            newId = direccion.Id;
                            break;
                        }

                    // --- GRUPO: NO SOPORTADOS AUN ---
                    default:
                        logger.LogWarning("Intento de agregar un tipo de contacto no soportado: {TipoMedio} al cliente {Id}", tipoMedio, idCliente);
                        return Result<long>.Failure($"El tipo de medio '{tipoMedio}' no está soportado en esta operación.");
                }

                logger.LogInformation("Contacto/Dirección tipo {TipoMedio} agregado correctamente al cliente {ClienteId}. Nuevo ID: {NewId}",
                                      tipoMedio, idCliente, newId);

                return Result<long>.Success(newId);
            }
            catch (Exception ex)
            {
                // El logeo de la excepción ya estaba bien, pero es buena práctica no exponer el error interno al usuario
                logger.LogError(ex, "Error crítico al intentar agregar el tipo de contacto {TipoMedio} al cliente {ClienteId}", tipoMedio, idCliente);
                return Result<long>.Failure("Ocurrió un error inesperado al guardar el contacto. Contacte a soporte técnico.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tipoContacto"></param>
        /// <param name="idCliente"></param>
        /// <param name="idContacto"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Result<string>> VerifyCustomerContactability(EnumQueryClientType tipoContacto,
                                                               long idCliente,
                                                               long idContacto,
                                                               CancellationToken cancellationToken = default)
        {
            try
            {
                Client? cliente = await _IClienteRepository.GetByIdAsync(idCliente, cancellationToken);
                if (cliente == null)
                {
                    return Result<string>.Failure($"No se encontró el cliente con ID {idCliente}");
                }

                // 1. Delegar la acción al Dominio. Si algo es inválido, lanzará una excepción.
                cliente.VerifyContactability(tipoContacto, idContacto, _usuarioActual);

                // 2. Si el dominio no se quejó, guardamos y limpiamos caché
                var changesCount = await unitOfWork.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Verificación guardada. Registros afectados: {Count}", changesCount);

                await _cacheService.RefreshSegmentAsync(cliente.Identification, tipoContacto, cancellationToken);

                return Result<string>.Success(cliente.Identification);
            }
            catch (InvalidOperationException ex)
            {
                // Atrapamos las reglas de negocio rotas (Ej: ID no existe en la lista)
                logger.LogWarning(ex, "Intento inválido de aprobación: Cliente {IdCliente}, Contacto {IdContacto}", idCliente, idContacto);
                return Result<string>.Failure(ex.Message);
            }
            catch (ArgumentException ex)
            {
                // Atrapamos argumentos inválidos (Ej: Tipo de contacto no soportado)
                return Result<string>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                // Atrapamos caídas de base de datos u otros errores críticos
                logger.LogError(ex, "Error crítico al actualizar el contacto {IdContacto} del cliente {IdCliente}", idContacto, idCliente);
                return Result<string>.Failure("Ocurrió un error interno al intentar actualizar la verificación.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tipoContacto"></param>
        /// <param name="idCliente"></param>
        /// <param name="idContacto"></param>
        /// <param name="errorCode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Result<string>> UnverifyCustomerContactability(EnumQueryClientType tipoContacto,
                                                                 long idCliente,
                                                                 long idContacto,
                                                                 EnumContactabilityStatus errorCode,
                                                                 CancellationToken cancellationToken = default)
        {
            try
            {
                Client? cliente = await _IClienteRepository.GetByIdAsync(idCliente, cancellationToken);
                if (cliente == null)
                {
                    return Result<string>.Failure($"No se encontró el cliente con ID {idCliente}");
                }
                cliente.UnverifyContactability(tipoContacto, idContacto, errorCode, _usuarioActual);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                await _cacheService.RefreshSegmentAsync(cliente.Identification, tipoContacto, cancellationToken);
                return Result<string>.Success(cliente.Identification);
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, "Intento inválido de rechazo: Cliente {IdCliente}, Contacto {IdContacto}", idCliente, idContacto);
                return Result<string>.Failure("Un parametro no fue el indicado, por favor revisar y volver a intentar.");
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Argumento inválido al rechazar contacto: Cliente {IdCliente}, Contacto {IdContacto}", idCliente, idContacto);
                return Result<string>.Failure("Un parametro no fue el indicado, por favor revisar y volver a intentar.");
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Error crítico al rechazar el contacto {IdContacto} del cliente {IdCliente}", idContacto, idCliente);
                return Result<string>.Failure("Ocurrió un error interno al intentar rechazar la verificación.");
            }
        }

        public async Task<Result> UpdateGpsAsync(long idClient, long idAddr, decimal? lat, decimal? lng)
        {
            var cliente = await _IClienteRepository.GetByIdAsync(idClient);


            if (cliente is null)
            {
                return Result.Failure($"No se encontró el cliente con ID {idClient}");
            }


            cliente.UpdateGPS(lat, lng, idAddr);

            await unitOfWork.SaveChangesAsync();

            return Result.Success();
        }


    }

}
