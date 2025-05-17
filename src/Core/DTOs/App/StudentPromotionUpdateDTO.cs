namespace Core.DTOs.App
{
    public class StudentPromotionUpdateDTO
    {
        public int StudentId { get; set; }
        public int GradeId { get; set; }
        public string Section { get; set; }
        public DateTime PromotionDate { get; set; }
        public int ModifyBy { get; set; }
    }

}