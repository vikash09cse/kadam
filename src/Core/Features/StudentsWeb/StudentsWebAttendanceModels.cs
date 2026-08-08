namespace Core.Features.StudentsWeb;

public static class StudentsWebAttendance
{
    public const int Present = 1;
    public const int Absent = 2;
    public const int Holiday = 3;
    public const int WorkingDay = 1;
    public const int HolidayDay = 2;

    // Keep these values aligned with the mobile attendance reason choices.
    public static readonly IReadOnlyList<string> AbsenceReasons =
    [
        "Health related issue",
        "Family function",
        "Out of station",
        "Household chores",
        "Paid work",
        "Others"
    ];
}

public sealed class StudentsWebAttendanceRowDTO
{
    public int StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string FatherName { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }
    public int? AttendanceStatus { get; set; }
    public string? AttendanceNote { get; set; }
    public byte? DateEntryPoint { get; set; }
}

public sealed class StudentsWebAttendanceSaveDTO
{
    public int InstitutionId { get; set; }
    public int GradeId { get; set; }
    public string Section { get; set; } = string.Empty;
    public DateTime AttendanceDate { get; set; }
    public int DayType { get; set; } = StudentsWebAttendance.WorkingDay;
    public List<StudentsWebAttendanceEntryDTO> Entries { get; set; } = [];
}

public sealed class StudentsWebAttendanceEntryDTO
{
    public int StudentId { get; set; }
    public int AttendanceStatus { get; set; }
    public string? AttendanceNote { get; set; }
}

public enum StudentsWebAttendanceSaveStatus
{
    Saved,
    InvalidScope,
    InvalidStudents
}
