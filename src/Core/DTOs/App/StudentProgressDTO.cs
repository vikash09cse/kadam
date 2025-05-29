namespace Core.DTOs.App
{
    public class StudentProgressDTO
    {
        public int StudentId { get; set; }
        public int GradeEntryLevelId { get; set; }
        public int GradeExitLevelId { get; set; }
        public int EntryStepId { get; set; }
        public int ExitStepId { get; set; }
        public int? LastCompletedStepId { get; set; }
        public string EntryLevelText { get; set; } = string.Empty;
        public string ExitLevelText { get; set; } = string.Empty;
        public string EntryStepText { get; set; } = string.Empty;
        public string ExitStepText { get; set; } = string.Empty;
        public string LastCompletedStepText { get; set; } = string.Empty;
        public IEnumerable<StudentGradeLevelProgressDTO> GradeLevelProgress { get; set; } = [];
        public IEnumerable<StudentBaselineDetailWithSubjectDTO> Baselines { get; set; } = [];
        public IEnumerable<StudentBaselineDetailWithSubjectDTO> EndBaselines { get; set; } = [];
    }

    public class StudentGradeLevelProgressDTO
    {
        public int GradeLevelId { get; set; }
        public string GradeLevelText { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public bool IsEnabled { get; set; }
        public string BackgroundColor { get; set; } = "#444040";

        public IEnumerable<StudentGradeStepProgressDTO> Steps { get; set; } = [];
    }
    public class StudentGradeStepProgressDTO
    {
        public int StepId { get; set; }
        public string StepText { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public string BackgroundColor {  get; set; } = "#444040";
    }
}
