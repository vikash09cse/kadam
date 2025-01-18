using Core.Entities;

namespace Core.DTOs
{
    public class DistrictListDTO : BaseEntity
    {
        public int RowNumber { get; set; }
        public string DistrictName { get; set; } = string.Empty;
        public string DistrictCode { get; set; } = string.Empty;
        public string StateName { get; set; } = string.Empty;
        public int TotalCount { get; set; }
    }
}
