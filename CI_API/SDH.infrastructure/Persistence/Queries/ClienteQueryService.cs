using Microsoft.EntityFrameworkCore;
using SDH.Application.DTOs.Clients;
using SDH.Application.Ports.Services;
using SDH.Domain.Entities.Operative;
using SDH.Domain.Enums;
using SDH.Domain.Extensions;
using SDH.infrastructure.Persistence.Data;

namespace SDH.infrastructure.Persistence.Queries
{
    public class ClienteQueryService(ApplicationDbContext context) : IClienteQueryService
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<IReadOnlyList<CustomerSearchResultDto>> LightSearchByNameAndIdentificationAsync(string termino, int top, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(termino))
            {
                return [];
            }

            // 2. Limpieza del input
            string terminoLimpio = termino.Trim();

            return await _context.Client
                .AsNoTracking()
                // 3. Orden de evaluación en el OR (Primero el más rápido)
                .Where(c =>
                        (
                            c.Identification.StartsWith(terminoLimpio) ||
                            (c.FullName != null && c.FullName.Contains(terminoLimpio))
                        )
                        && !c.IsDeleted
                        && !c.IsAnonymized
                    )
                .OrderBy(c => c.FullName)
                .Take(top)
                .Select(c => new CustomerSearchResultDto(
                    c.Id,
                    c.Identification,
                    c.FullName ?? "",
                    c.IsVerified
                ))
                .ToListAsync(cancellationToken);
        }

        public async Task<List<CustomerContactDto>> ObtenerContactosAsync(string cedula, CancellationToken cancellationToken = default)
        {
            return await _context.Client
                .AsNoTracking() // Excelente práctica para consultas de solo lectura
                .Where(c => c.Identification == cedula && !c.IsDeleted)
                .SelectMany(c => c.Contacts) // Aplana la lista: hace el JOIN a la tabla Contactos
                .Where(ct => !ct.IsDeleted) // Filtramos para traer solo los contactos activos
                .Select(ct => new CustomerContactDto(
                    ct.Id, // Recuerda que lo renombramos de IdContactoCliente a simplemente Id
                    ct.ContactMediumTypeId,
                    ct.ContactMediumType.GetValueCatalog(),
                    ct.ContactValue,
                    ct.VerifyStatusId,
                    ct.VerifyStatus.GetValueCatalog(),
                    ct.IsDeleted // Aunque siempre será false por el Where de arriba
                ))
                .ToListAsync(cancellationToken);
        }

        public async Task<CustomerAddressDto?> ObtenerDireccionPrincipalAsync(string cedula, CancellationToken cancellationToken = default)
        {
            return await _context.Client
                .AsNoTracking()
                .Where(c => c.Identification == cedula)
                .SelectMany(c => c.Addresses) // Aplana hacia la tabla hija DireccionCliente
                .Where(d => d.IsPrimary == true) // Filtramos solo la principal
                .Select(d => new CustomerAddressDto(
                    d.Id, // Renombrado de IdDireccionCliente a Id en el Dominio
                    d.AddressTypeId ?? (int)EnumContactabilityType.HomeAddress,
                    d.AddressType.GetValueCatalog(),
                    d.FullAddress,
                    d.City,
                    d.Province,
                    d.PostalCode,
                    d.Country,
                    d.Parish,
                    d.Latitude,
                    d.Longitude,
                    d.VerificationStatusId,
                    d.VerificationStatus.GetValueCatalog(),
                    d.Source,
                    d.IsPrimary
                ))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<CustomerAddressDto>> ObtenerDireccionesAsync(string cedula, CancellationToken cancellationToken = default)
        {
            // Ojo: Cambié la firma de List<DireccionDto?> a List<DireccionDto>
            // Las listas no deberían contener valores nulos por diseño.
            return await _context.Client
                .AsNoTracking()
                .Where(c => c.Identification == cedula)
                .SelectMany(c => c.Addresses) // Aplana todas las direcciones del cliente
                .Select(d => new CustomerAddressDto(
                    d.Id,
                    d.AddressTypeId ?? (int)EnumContactabilityType.HomeAddress,
                    d.AddressType.GetValueCatalog(),
                    d.FullAddress,
                    d.City,
                    d.Province,
                    d.PostalCode,
                    d.Country,
                    d.Parish,
                    d.Latitude,
                    d.Longitude,
                    d.VerificationStatusId,
                    d.VerificationStatus.GetValueCatalog(),
                    d.Source,
                    d.IsPrimary
                ))
                .ToListAsync(cancellationToken); // Corregido: Era ToList, ahora es ToListAsync
        }

        public async Task<Client?> ObtenerPorIdAsync(long id, CancellationToken cancellationToken = default)
        {
            return await _context.Client
                .AsNoTracking()
                .Where(c => !c.IsDeleted)
                .Include(c => c.Contacts.Where(ct => !ct.IsDeleted))
                .Include(c => c.Addresses.Where(cd => !cd.IsDeleted))
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<CustomerDto?> ObtenerPorIdentificacionAsync(string identificacion, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(identificacion)) return null;

            return await _context.Client
                .AsNoTracking()
                .Where(c => c.Identification == identificacion && !c.IsDeleted)
                .Select(c => new CustomerDto(
                        c.Id,
                        c.Identification,
                        c.IsVerified,
                        c.FullName,
                        c.Contacts
                            .Where(ct => !ct.IsDeleted)
                            .Select(ct => new CustomerContactDto
                            (
                                ct.Id,
                                ct.ContactMediumTypeId,
                                ct.ContactMediumType.GetValueCatalog(),
                                ct.ContactValue,
                                ct.VerifyStatusId,
                                ct.VerifyStatus.GetValueCatalog(),
                                ct.IsDeleted
                            )).ToList(),
                        c.Addresses
                            .Where(cd => !cd.IsDeleted)
                            .Select(cd => new CustomerAddressDto
                            (
                                cd.Id,
                                cd.AddressTypeId ?? (int)EnumContactabilityType.HomeAddress,
                                cd.AddressType.GetValueCatalog(),
                                cd.FullAddress,
                                cd.City,
                                cd.Province,
                                cd.PostalCode,
                                cd.Parish,
                                cd.Country,
                                cd.Latitude,
                                cd.Longitude,
                                cd.VerificationStatusId,
                                cd.VerificationStatus.GetValueCatalog(),
                                cd.Source,
                                cd.IsPrimary
                             )).ToList()
                    ))
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
