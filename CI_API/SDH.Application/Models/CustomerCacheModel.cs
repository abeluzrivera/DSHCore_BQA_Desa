using SDH.Application.DTOs.Clients;
using SDH.Domain.Enums.LogicaNegocio;

namespace SDH.Application.Models;

/// <summary>
/// Contenedor hidratable por usuario en IMemoryCache.
/// Arranca con datos básicos (Id, Cédula, Nombres) y sólo carga
/// los segmentos de información adicional cuando son requeridos.
/// 
/// Ciclo de vida: 15 minutos de expiración absoluta.
/// Thread-safety: las escrituras sobre las propiedades lazy están
/// protegidas por un SemaphoreSlim gestionado en ClienteCacheService.
/// </summary>
public sealed class CustomerCacheModel
{
    // ── Datos básicos (siempre presentes) ─────────────────────
    public long Id { get; init; }
    public string Identification { get; init; } = string.Empty;
    public string? FullName { get; init; } = string.Empty;
    public bool IsVerified { get; init; }

    // ── Datos extendidos (lazy — null hasta ser hidratados) ────
    public List<CustomerContactDto>? Contacts { get; set; }
    public CustomerFinancialDto? Financial { get; set; }
    public List<CustomerAddressDto>? Addresses { get; set; }

    // ── Metadata ───────────────────────────────────────────────
    public DateTime LastUpdated { get; private set; } = DateTime.Now;

    // ── Helpers ────────────────────────────────────────────────

    /// <summary>Indica si el segmento solicitado ya fue cargado.</summary>
    public bool HasSegment(EnumQueryClientType tipo) => tipo switch
    {
        EnumQueryClientType.Contact => Contacts is not null,
        EnumQueryClientType.Financiero => Financial is not null,
        EnumQueryClientType.Address => Addresses is not null,
        _ => false,
    };

    /// <summary>Inyecta el resultado de hidratación y actualiza la marca de tiempo.</summary>
    public void Hydrate(EnumQueryClientType tipo, object segmento)
    {
        switch (tipo)
        {
            case EnumQueryClientType.Contact:
                Contacts = segmento as List<CustomerContactDto>;
                break;
            case EnumQueryClientType.Financiero:
                Financial = segmento as CustomerFinancialDto;
                break;
            case EnumQueryClientType.Address:
                Addresses = segmento as List<CustomerAddressDto>;
                break;
        }
        LastUpdated = DateTime.Now;
    }


    /// <summary>Factory desde el DTO liviano de búsqueda.</summary>
    public static CustomerCacheModel FromSearchResult(CustomerSearchResultDto dto) =>
        new()
        {
            Id = dto.Id,
            Identification = dto.Identification,
            FullName = dto.FullName,
            IsVerified = dto.IsVerified,
        };
}
