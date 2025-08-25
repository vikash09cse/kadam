namespace Core.DTOs.App
{
    public class StudentAttendanceSaveResponseDTO
    {
        public int RowsAffected { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
