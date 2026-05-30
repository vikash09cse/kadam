namespace Core.Entities
{
    public class StudentDeleteLog
    {
        public int Id { get; set; }
        public int StudentRecordId { get; set; }
        public string KadamStudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public int? InstitutionId { get; set; }
        public string InstitutionName { get; set; } = string.Empty;
        public int DeletedBy { get; set; }
        public string DeletedByName { get; set; } = string.Empty;
        public DateTime DeletedDate { get; set; }
    }
}
