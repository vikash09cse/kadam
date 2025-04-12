namespace Core.Entities
{
    public class StudentDocument : BaseAuditableEntity
    {
        public int StudentId { get; set; }
        public int DocumentTypeId { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public string DocumentPath { get; set; } = string.Empty;
    }
} 