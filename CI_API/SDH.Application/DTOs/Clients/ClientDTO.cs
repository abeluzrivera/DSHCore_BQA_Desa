namespace SDH.Application.DTOs.Clients
{
    // ──────────────────────────────────────────────────
    // DTOs de Lectura (Read) - Usamos records posicionales
    // ──────────────────────────────────────────────────

    public record CustomerSearchResultDto(long Id, string Identification, string FullName, bool IsVerified);

    public record CustomerContactDto(int Id, int ContactMediumTypeId, string ContactMediumType, string ContactValue, int VerificationStatusId, string VerificationStatus, bool IsDeleted);

    public record CustomerDto(long Id, string Identification, bool IsVerified, string? FullName, List<CustomerContactDto> Contacts, List<CustomerAddressDto> Addresses);

    public record FinancialItemDto(long Id, string AccountingType, decimal AccountingAmount);

    public record CustomerFinancialDto(string Identification, List<FinancialItemDto> FinancialItems);

    public record CustomerAddressDto(
        int Id, int AddressTypeId, string AddressType, string? FullAddress,
        string? City, string? Province, string? PostalCode, string? Country,
        string? Parish, decimal? Latitude,
        decimal? Longitude, int? VerificationStatusId, string VerificationStatus, string? Source, bool? IsPrimary);

    // ──────────────────────────────────────────────────
    // DTOs de Escritura (Write / Commands)
    // ──────────────────────────────────────────────────

    public record UpdateAddressCommand(
        string? FullAddress, string? City, string? Province,
        string? PostalCode, string? ModifiedBy, decimal? Latitude, decimal? Longitude);

    /// <summary>Estado de verificacion calculado en API. Regla OR: al menos 1 grupo verificado. GroupState worst-wins.</summary>
    public record ContactVerificationStateDto(bool IsClientVerified, string PhoneState, string EmailState, string AddressState);
}
