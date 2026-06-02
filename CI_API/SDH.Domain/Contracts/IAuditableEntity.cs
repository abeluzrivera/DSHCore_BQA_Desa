namespace SDH.Domain.Contracts
{
    public interface IAuditableEntity
    {
        public string? LastModifiedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
    }
}