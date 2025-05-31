using System;

namespace Core.Entities
{
    public class StudentGradeTestDetail : BaseAuditableEntity
    {
        public int StudentId { get; set; }
        public int GradeLevelId { get; set; }
        public int SubjectId { get; set; }
        public int? StudentAge { get; set; }
        public decimal? ObtainedMarks { get; set; }
        public decimal? PercentageMarks { get; set; }
        public decimal? TotalMarks { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public int RowNo { get; set; } = 0;
    }
}