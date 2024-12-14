namespace Core.Entities
{
    public class Users
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public byte[] PasswordHash { get; set; } 
        public byte[] PasswordSalt { get; set; } 
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AlternatePhone { get; set; } = string.Empty;
        public int? Gender { get; set; }
        public string? Grade { get; set; }
        public string? Section { get; set; }
        public string? GradeSection { get; set; }
        public int? DivisionId { get; set; }
        public int RoleId { get; set; }
        public int ReporteeRoleId { get; set; }
        public int UserStatus { get; set; }
        public string? ActivityType { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DateCreated { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifyDate { get; set; }
        public int? ModifyBy { get; set; }
        public DateTime? DeletedDate { get; set; }
        public int? DeletedBy { get; set; }
    }
}
