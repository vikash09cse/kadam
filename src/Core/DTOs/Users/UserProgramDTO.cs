namespace Core.DTOs.Users
{
    public class UserProgramDTO
    {
        public int UserId { get; set; }
        public int ProgramId { get; set; }
        public string ProgramName { get; set; }= string.Empty;
        public bool IsSelected { get; set; }
    }
}
