namespace Core.DTOs.App
{
    public class StudentBaselineDetailWithSubjectDTO
    {
        public int RowNo { get; set; }
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        public int? StudentAge { get; set; }
        public string BaselineType { get; set; } = string.Empty;
        public decimal? ObtainedMarks { get; set; }
        public decimal? PercentageMarks { get; set; }
        public decimal? TotalMarks { get; set; }
        public int CurrentStatus { get; set; }
        public string SubjectName { get; set; } = string.Empty;
    }

    public class StudentBaselineDetailWithSubjectSaveDTO
    {
        public StudentBaselineDetailWithSubjectSaveDTO()
        {
            StudentBaselineDetails = new List<StudentBaselineDetailWithSubjectDTO>();
        }

        public int StudentId { get; set; }
        public List<StudentBaselineDetailWithSubjectDTO> StudentBaselineDetails { get; set; }
    }

} 