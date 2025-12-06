namespace Core.DTOs.App
{
    public class ThemeActivityListDTO
    {
        public int Id { get; set; }
        public int ThemeId { get; set; }
        public string ThemeName { get; set; } = string.Empty;
        public int InstitutionId { get; set; }
        public string InstitutionName { get; set; } = string.Empty;
        public string GradeId { get; set; } = string.Empty; // Comma-separated GradeIds
        public string GradeName { get; set; } = string.Empty; // Comma-separated GradeNames
        public string Section { get; set; } = string.Empty; // Comma-separated GradeNames with Sections (e.g., "Grade1 - Section1, Grade2 - Section2")
        public int TotalStudents { get; set; }
        public int StudentAttended { get; set; }
        public bool DidChildrenDayHappen { get; set; }
        public int? TotalParentsAttended { get; set; }
        public DateTime DateCreated { get; set; }
        public int CreatedBy { get; set; }
    }
}