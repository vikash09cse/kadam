using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
