using SDH.Domain.Enums;

namespace SDH.Application.DTOs.Clients
{

    public class AddContactRequest
    {
        public EnumContactabilityType ContactType { get; set; }
        public string ContactValue { get; set; } = string.Empty;
    }
}
