namespace Core.Entities
{
    public class ThemeActivity : BaseAuditableEntity
    {
        public int ThemeId { get; set; }
        public int InstitutionId { get; set; }
        public int GradeId { get; set; }
        public string Section { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public int StudentAttended { get; set; }
        public bool DidChildrenDayHappen { get; set; } = false;
        public int? TotalParentsAttended { get; set; }
    }
}