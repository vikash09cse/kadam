using Core.Entities;

namespace Core.DTOs.App
{
    public class StudentGradeTestDetailWithSubjectDTO
    {
        public int RowNo { get; set; }
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int GradeLevelId { get; set; }
        public int SubjectId { get; set; }
        public int? StudentAge { get; set; }
        public decimal? ObtainedMarks { get; set; }
        public decimal? PercentageMarks { get; set; }
        public decimal? TotalMarks { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public DateTime? CompletedDate { get; set; }
    }

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
