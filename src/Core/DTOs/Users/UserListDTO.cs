using Core.Utilities;

namespace Core.DTOs.Users
{
    public class UserListDTO
    {
        public int RowNumber { get; set; }
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string Phone { get; set; } = string.Empty;
        public Enums.Gender Gender { get; set; }
        public string GenderName => EnumHelper<Enums.Gender>.GetDescription(Gender);
        public string RoleName { get; set; } = string.Empty;
        public string ReporteeRoleName { get; set; } = string.Empty;
        public int TotalCount { get; set; }
    }
}
