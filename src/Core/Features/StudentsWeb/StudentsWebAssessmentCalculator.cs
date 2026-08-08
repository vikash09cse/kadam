namespace Core.Features.StudentsWeb;

public static class StudentsWebAssessmentCalculator
{
    public static decimal GetTotalMarks(bool isKadamPlusStudent, string gradeName, int age)
    {
        var level = GetLevel(isKadamPlusStudent, gradeName, age);
        return level * 10m;
    }

    public static decimal GetPercentage(decimal obtainedMarks, decimal totalMarks) =>
        Math.Round(obtainedMarks / totalMarks * 100m, 0, MidpointRounding.ToEven);

    public static StudentsWebPlacement GetPlacement(
        bool isKadamPlusStudent, string gradeName, int age, IEnumerable<decimal> obtainedMarks)
    {
        var level = GetLevel(isKadamPlusStudent, gradeName, age);
        var score = Math.Clamp(obtainedMarks.Sum(), 0m, 200m);
        var entryStep = score switch
        {
            <= 60m => 1,
            <= 100m => 3,
            <= 140m => 5,
            <= 180m => 7,
            _ => 9
        };
        entryStep = Math.Min(entryStep, (level * 2) - 1);
        var exitStep = level * 2;

        return new StudentsWebPlacement
        {
            GradeEntryLevelId = (entryStep + 1) / 2,
            GradeExitLevelId = level,
            EntryStepId = entryStep,
            ExitStepId = exitStep
        };
    }

    private static int GetLevel(bool isKadamPlusStudent, string gradeName, int age)
    {
        if (!isKadamPlusStudent)
            return age switch { <= 6 => 1, 7 => 2, 8 => 3, 9 => 4, _ => 5 };

        var normalized = gradeName.Trim().ToLowerInvariant();
        if (normalized.StartsWith("1")) return 1;
        if (normalized.StartsWith("2")) return 2;
        if (normalized.StartsWith("3")) return 3;
        if (normalized.StartsWith("4")) return 4;
        if (normalized.StartsWith("5")) return 5;
        throw new InvalidOperationException("The student's grade is not supported for baseline assessment.");
    }
}
