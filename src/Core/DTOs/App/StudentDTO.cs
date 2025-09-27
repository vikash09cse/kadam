using Core.Entities;

namespace Core.DTOs.App
{
    public class StudentDTO: Student
    {
        public bool IsBaselineAdded { get; set; }
    }
}
