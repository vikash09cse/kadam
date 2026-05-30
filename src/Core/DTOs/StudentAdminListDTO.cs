namespace Core.DTOs
{
    public class StudentAdminListDTO : BaseListDTO
    {
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string InstitutionName { get; set; } = string.Empty;
        public string GradeName { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public int Age { get; set; }
        public DateTime? EnrollmentDate { get; set; }
        public string StudentRegistratioNumber { get; set; } = string.Empty;
    }
}
