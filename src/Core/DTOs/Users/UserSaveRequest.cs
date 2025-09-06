namespace Core.DTOs.Users
{
    public class UserSaveRequest
    {
        public Core.Entities.Users User { get; set; } = new Core.Entities.Users();
        public string? Password { get; set; }
    }
}
