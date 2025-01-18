namespace Core.Entities
{
    public class InstitutionGradeSection
    {
        public int Id { get; set; }
        public int InstitutionId { get; set; }
        public int GradeId { get; set; }
        public string Sections { get; set; } = string.Empty;
    }
}
