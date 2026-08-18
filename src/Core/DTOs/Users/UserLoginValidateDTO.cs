namespace Core.DTOs.Users
{
    public class UserLoginValidateDTO
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public Utilities.Enums.PortalType PortalType { get; set; } = Utilities.Enums.PortalType.Admin;
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int ReporteeRoleId { get; set; }
        public string UserName { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public string Token { get; set; }  = string.Empty;
    }
}
