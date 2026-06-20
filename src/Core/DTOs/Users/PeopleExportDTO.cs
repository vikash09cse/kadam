namespace Core.DTOs.Users
{
    public class PeopleExportDTO
    {
        public int SrNo { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string UserName { get; set; } = string.Empty;
        public string GenderName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AlternatePhone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string ReporteeRoleName { get; set; } = string.Empty;
        public string? LastGeneratedPassword { get; set; }
        public string AssignedInstitutions { get; set; } = string.Empty;
    }
}
