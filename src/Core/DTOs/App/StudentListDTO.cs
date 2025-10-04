namespace Core.DTOs.App
{
    public class StudentListDTO
    {
        public int Id { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string AadhaarCardNumber { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string EnrollmentDate { get; set; } = string.Empty;
        public int CurrentStatus { get; set; }
        public int? TrioId { get; set; }
        public bool IsBaselineAdded { get; set; }
    }
}
