namespace Core.Entities
{
    public class StudentProgressStep
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int StepId { get; set; }
        public bool IsCompleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}