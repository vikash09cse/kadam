namespace Core.Entities
{
    public class State : BaseAuditableEntity
    {
        public string StateName { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
    }
}
