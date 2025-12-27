namespace Core.DTOs.App
{
    public class StudentListInstitutionMobileDTO
    {
        public int SrNo { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public DateTime? EnrollmentDate { get; set; }
        public int Age { get; set; }
        public string GradeName { get; set; } = string.Empty;
        public Utilities.Enums.Status CurrentStatus { get; set; }
        public string CurrentStatusText => CurrentStatus.ToString();
        public int Id { get; set; }
        public int IsBaselineAdded { get; set; }
        public int IsEndBaselineAdded { get; set; }
        public bool IsKadamPlusStudent { get; set; }
        public int StudentProgressStepCount { get; set; }
        public int ExitStepId { get; set; }
        public bool AllStepsCompleted => StudentProgressStepCount == ExitStepId;
        public DateTime? BaselineCompletedDate { get; set; }
        public DateTime? EndlineCompletedDate { get; set; }

    }
} 