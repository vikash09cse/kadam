namespace Core.Entities
{
    public class StudentTrio
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int TrioId { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public int? ModifyBy { get; set; }
        public DateTime? ModifyDate { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
