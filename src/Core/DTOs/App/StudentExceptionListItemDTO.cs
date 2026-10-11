namespace Core.DTOs.App
{
    public class StudentExceptionListItemDTO
    {
        public int StudentPk { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public int InstitutionId { get; set; }
        public string InstitutionName { get; set; } = string.Empty;
        public string DivisionName { get; set; } = string.Empty;
        public string StateName { get; set; } = string.Empty;
        public string GradeName { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public DateTime? EnrollmentDate { get; set; }
        public int CurrentStatus { get; set; }
        public string CurrentStatusText { get; set; } = string.Empty;
        public bool IsKadamPlusStudent { get; set; }
        public string ExceptionCode { get; set; } = string.Empty;
        public string ExceptionLabel { get; set; } = string.Empty;
        public int? LastProgressStepId { get; set; }
        public int? ExitStepId { get; set; }
        public DateTime? BaselineCompletedDate { get; set; }
        public DateTime? EndlineCompletedDate { get; set; }
        public DateTime? InActiveDate { get; set; }
        public int? DaysOverdue { get; set; }
        public int TotalCount { get; set; }
    }
}
