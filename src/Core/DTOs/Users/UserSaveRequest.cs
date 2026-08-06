namespace Core.DTOs.Users
{
    public class UserSaveRequest
    {
        public Core.Entities.Users User { get; set; } = new Core.Entities.Users();
        public string? Password { get; set; }
        /// <summary>When true, a new secure password is generated (manual password ignored).</summary>
        public bool AutoGeneratePassword { get; set; }
        /// <summary>Division assignments for DO/SPM roles.</summary>
        public List<int> DivisionIds { get; set; } = [];
    }
}
