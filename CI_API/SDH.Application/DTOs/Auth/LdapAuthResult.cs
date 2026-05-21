namespace SDH.Application.DTOs.Auth;

public record LdapAuthResult(
    bool Success,
    string? MappedRole,
    string? FullName,
    string? Email,
    string? Username,
    string? FailureReason
);
