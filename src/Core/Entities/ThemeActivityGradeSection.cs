namespace Core.Entities;

public sealed class ThemeActivityGradeSection
{
    public int Id { get; set; }
    public int ThemeActivityId { get; set; }
    public int GradeId { get; set; }
    public string? Section { get; set; }
}
