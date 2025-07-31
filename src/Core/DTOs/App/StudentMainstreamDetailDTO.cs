namespace Core.DTOs.App
{
    public class StudentMainstreamDetailDTO
    {
        public int StudentId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int InstitutionId { get; set; }
        public string InstitutionName { get; set; } = string.Empty;
        public string MainstreamInstitutionName { get; set; } = string.Empty;
        public string EnrolledGrade {  get; set; } = string.Empty;
        public int GradeId { get; set; }
        public string Section { get; set; } = string.Empty;
        public int StateId { get; set; }
        public int DistrictId { get; set; }
        public string InstitutionCode { get; set; } = string.Empty;
    }
}