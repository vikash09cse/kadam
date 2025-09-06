using System;

namespace Core.Entities
{
    
    public class StudentBaselineDetail: BaseAuditableEntity
    {
        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        public int? StudentAge { get; set; }
        public string BaselineType { get; set; }
        public decimal? ObtainedMarks { get; set; }
        public decimal? PercentageMarks { get; set; }
        public decimal? TotalMarks { get; set; }
        public DateTime? CompletedDate { get; set; }
    }
} 