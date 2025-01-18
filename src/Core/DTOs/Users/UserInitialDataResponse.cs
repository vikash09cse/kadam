namespace Core.DTOs.Users
{
    public class UserInitialDataResponse
    {
        public List<DropdownDTO> Genders { get; set; } = [];
        public IEnumerable<DropdownDTO> Roles { get; set; } = [];
        public IEnumerable<DropdownDTO> ReportRoles { get; set; } = [];
        public Core.Entities.Users UserInfo { get; set; } = new Core.Entities.Users();
    }
}
