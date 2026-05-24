namespace Core.DTOs
{
    public class KadamProgrammeReportFilterDTO
    {
        public int? StateId { get; set; }
        public int? DivisionId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool IncludeAll { get; set; } = true;
        public bool IncludeKadam { get; set; }
        public bool IncludeKadamPlus { get; set; }
    }
}
