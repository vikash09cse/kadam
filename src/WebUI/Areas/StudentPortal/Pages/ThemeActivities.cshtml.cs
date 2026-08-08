using Core.Features.StudentsWeb;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.StudentPortal.Pages;

[ValidateAntiForgeryToken]
public sealed class ThemeActivitiesModel(
    StudentsWebService studentsService,
    AuthenticationService authenticationService)
    : StudentPortalPageModel(studentsService, authenticationService)
{
    private const string PageUrl = "/StudentPortal/ThemeActivities";

    public IReadOnlyList<StudentsWebLookupDTO> Institutions { get; private set; } = [];
    public IReadOnlyList<StudentsWebLookupDTO> Themes { get; private set; } = [];
    public DateTime DefaultFromDate => DateTime.Today.AddMonths(-1);
    public DateTime DefaultToDate => DateTime.Today;

    public async Task<IActionResult> OnGetAsync()
    {
        var denied = await RequirePageAsync(PageUrl);
        if (denied is not null) return denied;
        Institutions = await StudentsService.GetInstitutions(CurrentUserId);
        Themes = await StudentsService.GetActiveThemes();
        return Page();
    }

    public async Task<IActionResult> OnGetGradeSectionsAsync(int institutionId)
    {
        var denied = await RequirePageAsync(PageUrl);
        if (denied is not null)
            return new JsonResult(Array.Empty<object>()) { StatusCode = 403 };
        return new JsonResult(
            await StudentsService.GetThemeActivityGradeSections(CurrentUserId, institutionId));
    }

    public async Task<IActionResult> OnGetThemeActivitiesAsync(
        int draw, int start, int length, int? institutionId, int? themeId,
        int? gradeId, string? section, DateTime? fromDate, DateTime? toDate)
    {
        var denied = await RequirePageAsync(PageUrl);
        if (denied is not null)
            return new JsonResult(new { error = "Access denied." }) { StatusCode = 403 };

        length = Math.Clamp(length, 10, 100);
        if (fromDate.HasValue && toDate.HasValue && fromDate.Value.Date > toDate.Value.Date)
            return new JsonResult(new
            {
                draw,
                recordsTotal = 0,
                recordsFiltered = 0,
                data = Array.Empty<object>()
            });

        var rows = await StudentsService.GetThemeActivities(
            CurrentUserId, (Math.Max(start, 0) / length) + 1, length,
            institutionId, themeId, gradeId, section, fromDate, toDate);
        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return new JsonResult(new { draw, recordsTotal = total, recordsFiltered = total, data = rows });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var denied = await RequirePageAsync(PageUrl);
        if (denied is not null)
            return new JsonResult(new { message = "Access denied." }) { StatusCode = 403 };

        var result = await StudentsService.DeleteThemeActivity(id, CurrentUserId);
        return new JsonResult(result) { StatusCode = result.Success ? 200 : 404 };
    }
}
