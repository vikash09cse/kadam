namespace Core.Entities
{
    public class StudentFollowup : BaseAuditableEntity
    {
        public int? StudentId { get; set; }
        public DateTime FollowupDate { get; set; }
        public int InstitutionId { get; set; }
        public int GradeId { get; set; }
        public string? Section { get; set; }
        public string? InchargeName { get; set; }
        public string? InchargeContactNumber { get; set; }
        public string? IsChildSitTogether { get; set; }
        public int? LastMonthAttendanceCount { get; set; }
        public int? LastMonthWorkingDayCount { get; set; }
        public double? LastMonthAttendancePercentage { get; set; }
        public int? MaleStudentCount { get; set; }
        public int? FemaleStudentCount { get; set; }
        public int? TodayStudentPresentCount { get; set; }
        public int? TotalStudentCount { get; set; }
        public double? TotalStudentPercentage { get; set; }
    }
}
