using Core.Features.StudentsWeb;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.StudentPortal.Pages;

public sealed class MyInstitutionModel(
    StudentsWebService studentsService,
    AuthenticationService authenticationService)
    : StudentPortalPageModel(studentsService, authenticationService)
{
    public IReadOnlyList<StudentsWebLookupDTO> Institutions { get; private set; } = [];
    public DateTime DefaultFromDate => DateTime.Today.AddMonths(-12);
    public DateTime DefaultToDate => DateTime.Today;

    public async Task<IActionResult> OnGetAsync()
    {
        var denied = await RequirePageAsync("/StudentPortal/MyInstitution");
        if (denied is not null) return denied;
        Institutions = await StudentsService.GetInstitutions(CurrentUserId);
        return Page();
    }

    public async Task<IActionResult> OnGetGradeSectionsAsync(int institutionId)
    {
        var denied = await RequirePageAsync("/StudentPortal/MyInstitution");
        if (denied is not null) return new JsonResult(Array.Empty<object>()) { StatusCode = 403 };

        var allowed = await StudentsService.GetInstitutions(CurrentUserId);
        if (!allowed.Any(x => x.Id == institutionId))
            return new JsonResult(Array.Empty<object>()) { StatusCode = 403 };
        return new JsonResult(await StudentsService.GetGradeSections(institutionId));
    }

    public async Task<IActionResult> OnGetStudentsAsync(
        int draw, int start, int length, string? searchText, int? institutionId,
        int? gradeId, string? section, DateTime? fromDate, DateTime? toDate, int? status)
    {
        var denied = await RequirePageAsync("/StudentPortal/MyInstitution");
        if (denied is not null) return new JsonResult(new { error = "Access denied." }) { StatusCode = 403 };

        length = Math.Clamp(length, 10, 100);
        status = status is 1 or 2 or 3 ? status : null;
        if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            return new JsonResult(new { draw, recordsTotal = 0, recordsFiltered = 0, data = Array.Empty<object>() });

        var rows = await StudentsService.GetInstitutionStudents(
            CurrentUserId, (start / length) + 1, length, searchText,
            institutionId, gradeId, section, fromDate, toDate, status);
        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return new JsonResult(new { draw, recordsTotal = total, recordsFiltered = total, data = rows });
    }
}
