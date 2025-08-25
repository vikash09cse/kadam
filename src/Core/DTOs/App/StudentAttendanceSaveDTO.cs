namespace Core.DTOs.App
{
    public class StudentAttendanceSaveDTO
    {
        public int StudentId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public int AttendanceStatus { get; set; }
        public string? AttendanceNote { get; set; }
    }
}
