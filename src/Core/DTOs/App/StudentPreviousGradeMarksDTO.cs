namespace Core.DTOs.App
{
    public class StudentPreviousGradeMarksDTO
    {
        public bool PreviousGradeExists { get; set; }
        public decimal TotalObtainedMarks { get; set; }
        public decimal TotalMarks { get; set; }
        public decimal OverallPercentage { get; set; }
        public bool HasPassed80Percent { get; set; }
        public string Message { get; set; } = string.Empty;
        public int PreviousGradeLevelId { get; set; }
    }
}
