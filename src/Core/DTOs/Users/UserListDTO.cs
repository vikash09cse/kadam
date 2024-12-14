namespace Core.DTOs.Users
{
    public class UserListDTO
    {
        public int RowNumber { get; set; }
        public int Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string Phone { get; set; }
        public string Gender { get; set; }
        public int RoleId { get; set; }
        public int? ReporteeRoleId { get; set; }
        public int TotalCount { get; set; }
    }
}
