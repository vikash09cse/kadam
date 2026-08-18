namespace Core.DTOs.App
{
    public class StudentFollowupReportFilterDTO
    {
        public int? StateId { get; set; }
        public int? DivisionId { get; set; }
        public int? InstitutionId { get; set; }
        public int? GradeId { get; set; }
        public string? Section { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
