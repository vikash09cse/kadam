namespace Core.DTOs.App
{
    public class StudentStatusUpdateDTO
    {
        public int StudentId { get; set; }
        public int Status { get; set; }
        public string InActiveReason { get; set; } = string.Empty;
        public DateTime? InActiveDate { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public int UpdatedBy { get; set; }
    }
}
