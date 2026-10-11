namespace Core.DTOs.App
{
    public class StudentAnalyticsSummaryDTO
    {
        public int TotalStudents { get; set; }
        public int ActiveCount { get; set; }
        public int InactiveCount { get; set; }
        public int CompletedCount { get; set; }
        public int BaselineCompletedCount { get; set; }
        public decimal BaselineCompletedPercent { get; set; }
        public int ProgressStartedCount { get; set; }
        public decimal ProgressStartedPercent { get; set; }
        public int StepsCompletedCount { get; set; }
        public decimal StepsCompletedPercent { get; set; }
        public int EndlineCompletedCount { get; set; }
        public decimal EndlineCompletedPercent { get; set; }
        public int MainstreamedCount { get; set; }
        public decimal MainstreamedPercent { get; set; }
        public int MainstreamEligibleCount { get; set; }
        public int GradeTest1Count { get; set; }
        public int GradeTest2Count { get; set; }
        public int GradeTest3Count { get; set; }
        public int GradeTest4Count { get; set; }
        public int GradeTest5Count { get; set; }
        public int InProgressCount { get; set; }
        public int NoBaselineActiveCount { get; set; }
    }
}
