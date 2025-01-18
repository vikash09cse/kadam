using static Core.Utilities.Enums;

namespace Core.DTOs
{
    public class MenuPermissionListDTO
    {
        public int RowNumber { get; set; }
        public int Id { get; set; }
        public string MenuName { get; set; } = string.Empty;
        public string ParentMenuName { get; set; } = string.Empty;
        public Status CurrentStatus { get; set; } = Status.Active;
        public string CurrentStatusText => CurrentStatus.ToString();
        public int TotalCount { get; set; } = 0;
    }

    public class MenuPermissionDTO
    {
        public int Id { get; set; }
        public string MenuName { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}
