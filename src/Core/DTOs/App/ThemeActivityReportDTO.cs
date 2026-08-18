namespace Core.DTOs.App
{
    public class ThemeActivityReportDTO
    {
        public int Id { get; set; }
        public DateTime? ThemeActivityDate { get; set; }
        public int InstitutionId { get; set; }
        public string InstitutionName { get; set; } = string.Empty;
        public int ThemeId { get; set; }
        public string ThemeName { get; set; } = string.Empty;
        public string GradeSectionsText { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public int StudentAttended { get; set; }
        public bool DidChildrenDayHappen { get; set; }
        public int? TotalParentsAttended { get; set; }
        public int TotalParticipants { get; set; }
        public byte DateEntryPoint { get; set; }
        public string EntryPointText { get; set; } = string.Empty;
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
    }
}
