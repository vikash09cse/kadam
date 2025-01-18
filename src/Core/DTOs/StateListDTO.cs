using Core.Entities;

namespace Core.DTOs
{
    public class StateListDTO : BaseEntity
    {
        public int RowNumber { get; set; }
        public string StateName { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string CurrentStatusText => CurrentStatus.ToString();
        public int TotalCount { get; set; }
    }
}
