namespace Core.DTOs.App
{
    public class StudentFollowupListDTO
    {
        public int Id { get; set; }
        public int? StudentId { get; set; }
        public string? StudentName { get; set; } = string.Empty;
        public DateTime FollowupDate { get; set; }
        public int InstitutionId { get; set; }
        public string InstitutionName { get; set; } = string.Empty;
        public int GradeId { get; set; }
        public string GradeName { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string InchargeName { get; set; } = string.Empty;
        public string InchargeContactNumber { get; set; } = string.Empty;
        public string IsChildSitTogether { get; set; } = string.Empty;
        public int? LastMonthAttendanceCount { get; set; }
        public int? LastMonthWorkingDayCount { get; set; }
        public float? LastMonthAttendancePercentage { get; set; }
        public int? MaleStudentCount { get; set; }
        public int? FemaleStudentCount { get; set; }
        public int? TodayStudentPresentCount { get; set; }
        public int? TotalStudentCount { get; set; }
        public float? TotalStudentPercentage { get; set; }
        public DateTime DateCreated { get; set; }
        public int CreatedBy { get; set; }
    }
}
