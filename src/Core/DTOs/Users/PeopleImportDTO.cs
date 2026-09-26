namespace Core.DTOs.Users
{
    public class PeopleImportRowDTO
    {
        public int RowNumber { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AlternatePhone { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string ReporteeRole { get; set; } = string.Empty;
    }

    public class PeopleImportErrorDTO
    {
        public int RowNumber { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class PeopleImportCredentialDTO
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string GeneratedPassword { get; set; } = string.Empty;
    }

    public class PeopleImportResultDTO
    {
        public List<PeopleImportErrorDTO> Errors { get; set; } = [];
        public List<PeopleImportCredentialDTO> Credentials { get; set; } = [];
        public int Inserted { get; set; }
        public bool HasErrors => Errors.Count > 0;
    }
}
