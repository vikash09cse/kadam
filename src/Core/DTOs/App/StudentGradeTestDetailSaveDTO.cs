using Core.Entities;

namespace Core.DTOs.App
{
    public class StudentGradeTestDetailSaveDTO
    {
        public StudentGradeTestDetailSaveDTO()
        {
            StudentGradeTestDetails = new List<StudentGradeTestDetail>();
        }
        public int StudentId { get; set; }
        public int GradeLevelId { get; set; }
        public int CreatedBy { get; set; }
        public List<StudentGradeTestDetail> StudentGradeTestDetails { get; set; }
    }
}
