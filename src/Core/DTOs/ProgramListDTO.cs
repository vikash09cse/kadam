using static Core.Utilities.Enums;

namespace Core.DTOs
{
    public class ProgramListDTO
    {
        public int RowNumber { get; set; }
        public int Id { get; set; }
        public string ProgramName { get; set; } = string.Empty;
        public Status CurrentStatus { get; set; } = Status.Active;
        public string CurrentStatusText => CurrentStatus.ToString();
        public int TotalCount { get; set; } = 0;
    }
}
