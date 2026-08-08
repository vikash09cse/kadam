namespace Core.Features.StudentsWeb;

public enum StudentsWebAssessmentKind
{
    Baseline = 1,
    Endline = 2
}

public sealed class StudentsWebAssessmentDTO
{
    public int StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }
    public int Age { get; set; }
    public int GradeId { get; set; }
    public string GradeName { get; set; } = string.Empty;
    public bool IsKadamPlusStudent { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime? BaselineCompletedDate { get; set; }
    public bool HasProgress { get; set; }
    public bool IsLocked { get; set; }
    public string LockReason { get; set; } = string.Empty;
    public List<StudentsWebAssessmentSubjectDTO> Subjects { get; set; } = [];
}

public sealed class StudentsWebAssessmentSubjectDTO
{
    public int DetailId { get; set; }
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public decimal? ObtainedMarks { get; set; }
    public decimal TotalMarks { get; set; }
    public decimal? PercentageMarks { get; set; }
    public decimal? BaselineObtainedMarks { get; set; }
}

public sealed class StudentsWebAssessmentSaveDTO
{
    public int StudentId { get; set; }
    public DateTime? CompletedDate { get; set; }
    public List<StudentsWebAssessmentScoreInputDTO> Scores { get; set; } = [];
}

public sealed class StudentsWebAssessmentScoreInputDTO
{
    public int SubjectId { get; set; }
    public decimal? ObtainedMarks { get; set; }
}

public sealed class StudentsWebPlacement
{
    public int GradeEntryLevelId { get; init; }
    public int GradeExitLevelId { get; init; }
    public int EntryStepId { get; init; }
    public int ExitStepId { get; init; }
}

public enum StudentsWebAssessmentSaveStatus
{
    Saved,
    NotAuthorizedOrNotFound,
    Locked,
    InvalidSubjects
}

public sealed class StudentsWebProgressDTO
{
    public int StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string GradeName { get; set; } = string.Empty;
    public DateTime? BaselineCompletedDate { get; set; }
    public int GradeEntryLevelId { get; set; }
    public int GradeExitLevelId { get; set; }
    public int EntryStepId { get; set; }
    public int ExitStepId { get; set; }
    public int? LastCompletedStepId { get; set; }
    public List<StudentsWebProgressStepDTO> Steps { get; set; } = [];
    public List<int> CompletedGradeLevels { get; set; } = [];
}

public sealed class StudentsWebProgressStepDTO
{
    public int StepId { get; set; }
    public string StepText { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public bool IsInRange { get; set; }
}

public enum StudentsWebProgressSaveStatus
{
    Saved,
    NotAuthorizedOrNotFound,
    BaselineRequired,
    OutOfRange,
    PreviousStepRequired,
    PreviousGradeTestRequired
}

public sealed class StudentsWebGradeTestDTO
{
    public int StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public int GradeLevelId { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int EntryStepId { get; set; }
    public int ExitStepId { get; set; }
    public decimal? PreviousGradePercentage { get; set; }
    public List<StudentsWebAssessmentSubjectDTO> Subjects { get; set; } = [];
}

public sealed class StudentsWebGradeTestSaveDTO
{
    public int StudentId { get; set; }
    public int GradeLevelId { get; set; }
    public DateTime? CompletedDate { get; set; }
    public List<StudentsWebAssessmentScoreInputDTO> Scores { get; set; } = [];
}

public enum StudentsWebGradeTestSaveStatus
{
    Saved,
    NotAuthorizedOrNotFound,
    StepsIncomplete,
    PreviousGradeScoreRequired,
    InvalidSubjects
}
