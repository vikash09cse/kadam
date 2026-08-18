namespace Core.Features.StudentsWeb;

public sealed class StudentsWebThemeActivityGradeSectionDTO
{
    public int GradeId { get; set; }
    public string GradeName { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
}

public sealed class StudentsWebThemeActivitySaveDTO
{
    public int Id { get; set; }
    public DateTime ActivityDate { get; set; } = DateTime.Today;
    public int InstitutionId { get; set; }
    public int ThemeId { get; set; }
    public List<StudentsWebThemeActivityGradeSectionDTO> GradeSections { get; set; } = [];
    public int StudentsAttended { get; set; }
    public bool DidChildrensDayHappen { get; set; }
    public int? ParentsAttended { get; set; }
}

public class StudentsWebThemeActivityDTO
{
    public int Id { get; set; }
    public DateTime ActivityDate { get; set; }
    public int InstitutionId { get; set; }
    public string InstitutionName { get; set; } = string.Empty;
    public int ThemeId { get; set; }
    public string ThemeName { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public int StudentsAttended { get; set; }
    public bool DidChildrensDayHappen { get; set; }
    public int? ParentsAttended { get; set; }
    public int TotalParticipants => StudentsAttended + (ParentsAttended ?? 0);
    public byte DateEntryPoint { get; set; }
    public string EntryPointText => DateEntryPoint == StudentsWebEntryPoint.Web ? "Web" : "Mobile";
    public int CreatedBy { get; set; }
    public DateTime DateCreated { get; set; }
    public List<StudentsWebThemeActivityGradeSectionDTO> GradeSections { get; set; } = [];
}

public sealed class StudentsWebThemeActivityListItemDTO : StudentsWebThemeActivityDTO
{
    public long RowNumber { get; set; }
    public int TotalCount { get; set; }
    public string GradeSectionsText { get; set; } = string.Empty;
}

public enum StudentsWebThemeActivitySaveStatus
{
    Saved,
    NotAuthorizedOrNotFound,
    InvalidScope,
    InvalidTheme,
    InvalidDate,
    NoEligibleStudents,
    InvalidAttendance
}
