namespace Core.Entities
{
    public class District : BaseAuditableEntity
    {
        public string DistrictName { get; set; } = string.Empty;
        public string DistrictCode { get; set; } = string.Empty;
        public int StateId { get; set; } = 0;
    }
}
