namespace Core.Features.StudentsWeb;

public sealed class StudentsWebFollowupSaveDTO
{
    public int Id { get; set; }
    public DateTime VisitDate { get; set; }
    public int InstitutionId { get; set; }
    public int GradeId { get; set; }
    public string Section { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string TeacherContact { get; set; } = string.Empty;
    public int MaleStudentCount { get; set; }
    public int FemaleStudentCount { get; set; }
    public int PresentTodayCount { get; set; }
    public int? LastMonthWorkingDays { get; set; }
    public int? LastMonthAttendance { get; set; }
    public string? ChildrenSitTogether { get; set; }
}

public class StudentsWebFollowupDTO
{
    public int Id { get; set; }
    public DateTime VisitDate { get; set; }
    public int InstitutionId { get; set; }
    public string InstitutionName { get; set; } = string.Empty;
    public int GradeId { get; set; }
    public string GradeName { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string TeacherContact { get; set; } = string.Empty;
    public int MaleStudentCount { get; set; }
    public int FemaleStudentCount { get; set; }
    public int TotalStudentCount { get; set; }
    public int PresentTodayCount { get; set; }
    public double TodayAttendancePercentage { get; set; }
    public int? LastMonthWorkingDays { get; set; }
    public int? LastMonthAttendance { get; set; }
    public double? LastMonthAttendancePercentage { get; set; }
    public string? ChildrenSitTogether { get; set; }
    public byte DateEntryPoint { get; set; }
    public string EntryPointText => DateEntryPoint == StudentsWebEntryPoint.Web ? "Web" : "Mobile";
    public int CreatedBy { get; set; }
    public DateTime DateCreated { get; set; }
}

public sealed class StudentsWebFollowupListItemDTO : StudentsWebFollowupDTO
{
    public long RowNumber { get; set; }
    public int TotalCount { get; set; }
}

public enum StudentsWebFollowupSaveStatus
{
    Saved,
    NotAuthorizedOrNotFound,
    InvalidScope
}
