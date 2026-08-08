using Core.Features.StudentsWeb;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.StudentPortal.Pages;

[ValidateAntiForgeryToken]
public sealed class AttendanceModel(
    StudentsWebService studentsService,
    AuthenticationService authenticationService)
    : StudentPortalPageModel(studentsService, authenticationService)
{
    private const string PageUrl = "/StudentPortal/Attendance";

    public IReadOnlyList<StudentsWebLookupDTO> Institutions { get; private set; } = [];
    public IReadOnlyList<string> AbsenceReasons => StudentsWebAttendance.AbsenceReasons;
    public DateTime Today => DateTime.Today;

    public async Task<IActionResult> OnGetAsync()
    {
        var denied = await RequirePageAsync(PageUrl);
        if (denied is not null) return denied;
        Institutions = await StudentsService.GetInstitutions(CurrentUserId);
        return Page();
    }

    public async Task<IActionResult> OnGetGradeSectionsAsync(int institutionId)
    {
        var denied = await RequirePageAsync(PageUrl);
        if (denied is not null)
            return new JsonResult(new { error = "Access denied." }) { StatusCode = 403 };

        var allowed = await StudentsService.GetInstitutions(CurrentUserId);
        if (!allowed.Any(x => x.Id == institutionId))
            return new JsonResult(new { error = "Institution is inaccessible." }) { StatusCode = 403 };
        return new JsonResult(await StudentsService.GetGradeSections(institutionId));
    }

    public async Task<IActionResult> OnGetRosterAsync(
        int institutionId, int gradeId, string? section, DateTime? attendanceDate)
    {
        var denied = await RequirePageAsync(PageUrl);
        if (denied is not null)
            return new JsonResult(new { error = "Access denied." }) { StatusCode = 403 };

        if (institutionId <= 0 || gradeId <= 0 || string.IsNullOrWhiteSpace(section) ||
            !attendanceDate.HasValue || attendanceDate.Value.Date > DateTime.Today)
            return new JsonResult(new { error = "Select a valid institution, grade, section, and date." })
            {
                StatusCode = 400
            };

        var allowed = await StudentsService.GetInstitutions(CurrentUserId);
        if (!allowed.Any(x => x.Id == institutionId))
            return new JsonResult(new { error = "Institution is inaccessible." }) { StatusCode = 403 };
        var grades = await StudentsService.GetGradeSections(institutionId);
        var grade = grades.FirstOrDefault(x => x.Id == gradeId);
        var validSections = (grade?.Sections ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (grade is null || !validSections.Contains(section.Trim(), StringComparer.OrdinalIgnoreCase))
            return new JsonResult(new { error = "Grade or section is invalid." }) { StatusCode = 400 };

        var rows = await StudentsService.GetAttendanceRoster(
            CurrentUserId, institutionId, gradeId, section.Trim(), attendanceDate.Value.Date);
        return new JsonResult(rows);
    }

    public async Task<IActionResult> OnPostSaveAsync(
        [FromBody] StudentsWebAttendanceSaveDTO model)
    {
        var denied = await RequirePageAsync(PageUrl);
        if (denied is not null)
            return new JsonResult(new { message = "Access denied." }) { StatusCode = 403 };

        var result = await StudentsService.SaveAttendance(model, CurrentUserId);
        return new JsonResult(result) { StatusCode = result.Success ? 200 : 400 };
    }
}
