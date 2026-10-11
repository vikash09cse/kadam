namespace Core.DTOs
{
    public class StudentExceptionFilterDTO : KadamProgrammeReportFilterDTO
    {
        public string? ExceptionCode { get; set; }
        public int DaysNoBaseline { get; set; } = 30;
        public int DaysStuck { get; set; } = 14;
        public int DaysInactive { get; set; } = 30;
        public string GroupBy { get; set; } = "DIVISION";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
