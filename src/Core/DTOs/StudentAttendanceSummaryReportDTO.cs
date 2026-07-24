namespace Core.DTOs
{
    public class StudentAttendanceSummaryReportFilterDTO
    {
        public int InstitutionId { get; set; }
        public int? GradeId { get; set; }
        public string? Section { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class StudentAttendanceSummaryReportDTO
    {
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string InstitutionName { get; set; } = string.Empty;
        public string GradeName { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int HolidayCount { get; set; }
        public int WorkingDays { get; set; }
        public decimal AttendancePercent { get; set; }
    }
}
