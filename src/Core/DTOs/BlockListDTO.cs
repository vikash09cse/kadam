using Core.Entities;

namespace Core.DTOs
{
    public class BlockListDTO : BaseEntity
    {
        public int RowNumber { get; set; }
        public string BlockName { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public string StateName { get; set; } = string.Empty;
        public int TotalCount { get; set; }
    }
}
