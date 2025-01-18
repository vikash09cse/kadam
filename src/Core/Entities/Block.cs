namespace Core.Entities
{
    public class Block : BaseAuditableEntity
    {
        public string BlockName { get; set; } = string.Empty;
        public int DistrictId { get; set; } = 0;
        public int StateId { get; set; } = 0;
    }
}
