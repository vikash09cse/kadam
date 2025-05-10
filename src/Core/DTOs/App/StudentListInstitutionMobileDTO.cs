using System;

namespace Core.DTOs.App
{
    public class StudentListInstitutionMobileDTO
    {
        public int SrNo { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public string EnrollmentDate { get; set; } = string.Empty;
        public int Age { get; set; }
        public string GradeName { get; set; } = string.Empty;
        public Utilities.Enums.Status CurrentStatus { get; set; }
        public string CurrentStatusText => CurrentStatus.ToString();
        public int Id { get; set; }
    }
} 