namespace Core.Entities
{
    public class Village : BaseAuditableEntity
    {
        public string VillageName { get; set; } = string.Empty;
        public int BlockId { get; set; } = 0;
        public int DistrictId { get; set; } = 0;
        public int StateId { get; set; } = 0;
    }
}
