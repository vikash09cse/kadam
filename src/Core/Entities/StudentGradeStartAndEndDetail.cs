namespace Core.Entities;

public sealed class StudentGradeStartAndEndDetail
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int GradeEntryLevelId { get; set; }
    public int GradeExitLevelId { get; set; }
    public int EntryStepId { get; set; }
    public int ExitStepId { get; set; }
    public int? LastCompletedStepId { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? DateCreated { get; set; }
    public int? ModifyBy { get; set; }
    public DateTime? ModifyDate { get; set; }
    public byte DateEntryPoint { get; set; } = 1;
}
