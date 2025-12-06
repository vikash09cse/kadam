namespace Core.DTOs.App
{
    public class ThemeActivityDetailDTO
    {
        public int Id { get; set; }
        public int ThemeId { get; set; }
        public int InstitutionId { get; set; }
        public int TotalStudents { get; set; }
        public int StudentAttended { get; set; }
        public bool DidChildrenDayHappen { get; set; }
        public int? TotalParentsAttended { get; set; }
        public DateTime? ThemeActivityDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime DateCreated { get; set; }
        public int? ModifyBy { get; set; }
        public DateTime? ModifyDate { get; set; }
        public List<ThemeActivityGradeSectionDTO> GradeSections { get; set; } = new List<ThemeActivityGradeSectionDTO>();
    }
}

