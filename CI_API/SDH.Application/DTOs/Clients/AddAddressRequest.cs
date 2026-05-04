using SDH.Domain.Enums;

namespace SDH.Application.DTOs.Clients
{

    public class AddAddressRequest
    {
        public EnumContactabilityType AddressType { get; set; }
        public string FullAddress { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? Province { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? CountryCode { get; set; }
        public string? CityCode { get; set; }
        public string? ProvinceCode { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public bool? IsPrimary { get; set; }
    }
}
