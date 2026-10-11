namespace Core.DTOs.App
{
    public class StudentAnalyticsBreakdownDTO
    {
        public int DimensionId { get; set; }
        public string DimensionName { get; set; } = string.Empty;
        public string StateName { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public int ActiveCount { get; set; }
        public int InactiveCount { get; set; }
        public int CompletedCount { get; set; }
        public int BaselineCompletedCount { get; set; }
        public int ProgressStartedCount { get; set; }
        public int StepsCompletedCount { get; set; }
        public int EndlineCompletedCount { get; set; }
        public int MainstreamedCount { get; set; }
        public int MainstreamEligibleCount { get; set; }
        public decimal BaselineCompletedPercent { get; set; }
    }
}
