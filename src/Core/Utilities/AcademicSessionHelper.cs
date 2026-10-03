namespace Core.Utilities;

/// <summary>
/// Academic session helpers. Session years follow the Student ID convention
/// (calendar year Y maps to session "Y-(Y+1)").
/// </summary>
public static class AcademicSessionHelper
{
    public static DateTime GetPreviousSessionStart(DateTime asOf) =>
        new(asOf.Year - 1, 1, 1);

    /// <summary>
    /// Mainstream date may not be earlier than the previous session start,
    /// and not earlier than the student's enrollment date.
    /// </summary>
    public static DateTime GetMinimumMainstreamDate(DateTime asOf, DateTime enrollmentDate)
    {
        var previousSessionStart = GetPreviousSessionStart(asOf.Date);
        var enrollment = enrollmentDate.Date;
        return enrollment > previousSessionStart ? enrollment : previousSessionStart;
    }

    public static bool IsMainstreamDateInAllowedRange(
        DateTime mainstreamDate,
        DateTime asOf,
        DateTime enrollmentDate)
    {
        var date = mainstreamDate.Date;
        if (date > asOf.Date) return false;
        return date >= GetMinimumMainstreamDate(asOf, enrollmentDate);
    }
}
