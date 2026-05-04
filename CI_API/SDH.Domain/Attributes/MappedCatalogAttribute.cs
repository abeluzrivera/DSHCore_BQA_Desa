namespace SDH.Domain.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Enum)]
    public class MappedCatalog(string value) : Attribute
    {
        public string Value { get; } = value;
    }
}