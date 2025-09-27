namespace Core.Entities
{
    public class Subject : BaseAuditableEntity
    {
        public string SubjectName { get; set; } = string.Empty;
        public int DisplayOrder { get; set; } = 0;
        public int GradeTestTotalMarks { get; set; } = 0;
    }
}
