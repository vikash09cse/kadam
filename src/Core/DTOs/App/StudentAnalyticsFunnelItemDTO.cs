namespace Core.DTOs.App
{
    public class StudentAnalyticsFunnelItemDTO
    {
        public string StepKey { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
