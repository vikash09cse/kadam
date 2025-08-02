namespace Core.DTOs.App
{
    public class ThemeActivityListDTO
    {
        public int Id { get; set; }
        public int ThemeId { get; set; }
        public string ThemeName { get; set; } = string.Empty;
        public int InstitutionId { get; set; }
        public string InstitutionName { get; set; } = string.Empty;
        public int GradeId { get; set; }
        public string GradeName { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public int StudentAttended { get; set; }
        public bool DidChildrenDayHappen { get; set; }
        public int? TotalParentsAttended { get; set; }
        public DateTime DateCreated { get; set; }
        public int CreatedBy { get; set; }
    }
}