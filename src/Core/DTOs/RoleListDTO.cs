using static Core.Utilities.Enums;

namespace Core.DTOs
{
    public class RoleListDTO
    {
        public int RowNumber { get; set; }
        public int Id { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public Core.Utilities.Enums.PortalType PortalType { get; set; } = Core.Utilities.Enums.PortalType.Admin;
        public string PortalTypeText => PortalType == Core.Utilities.Enums.PortalType.Student ? "Student Portal" : "Admin Portal";
        public Status CurrentStatus { get; set; } = Status.Active;
        public string CurrentStatusText => CurrentStatus.ToString();
        public int TotalCount { get; set; } = 0;
    }
}
