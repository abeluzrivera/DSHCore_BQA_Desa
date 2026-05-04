namespace SDH.Application.DTOs.Clients
{

    // DTO Inmutable refactorizado usando Positional Record syntax
    public sealed record CustomerListItemDto(
        long Id,
        string FullName,
        string Identification,
        string VerificationStatus,
        bool IsVerified,
        string Type,
        string? PhotoUrl,
        bool HasPhone,
        bool HasEmail,
        bool HasAddress,
        string PhoneStatus,
        string EmailStatus,
        string AddressStatus
    );
}
