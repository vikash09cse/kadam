using Core.Features.StudentsWeb;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.StudentPortal.Pages;

public sealed class DirectoryModel(
    StudentsWebService studentsService,
    AuthenticationService authenticationService)
    : StudentPortalPageModel(studentsService, authenticationService)
{
    public async Task<IActionResult> OnGetAsync()
    {
        var denied = await RequirePageAsync("/StudentPortal/Directory");
        return denied ?? Page();
    }

    public async Task<IActionResult> OnGetStudentsAsync(
        int draw, int start, int length, string? studentName, string? studentId, string? aadhaarNumber, int? status)
    {
        var denied = await RequirePageAsync("/StudentPortal/Directory");
        if (denied is not null) return new JsonResult(new { error = "Access denied." }) { StatusCode = 403 };

        length = Math.Clamp(length, 10, 100);
        var rows = await StudentsService.GetStudents(
            CurrentUserId, (start / length) + 1, length, studentName, studentId, aadhaarNumber, status);
        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return new JsonResult(new { draw, recordsTotal = total, recordsFiltered = total, data = rows });
    }

    public async Task<IActionResult> OnPostStatusAsync([FromBody] StudentsWebStatusDTO model)
    {
        var denied = await RequirePageAsync("/StudentPortal/Directory");
        if (denied is not null) return new JsonResult(new { success = false, message = "Access denied." }) { StatusCode = 403 };
        return new JsonResult(await StudentsService.UpdateStatus(model, CurrentUserId));
    }

    public async Task<IActionResult> OnPostTrioAsync([FromBody] StudentsWebTrioDTO model)
    {
        var denied = await RequirePageAsync("/StudentPortal/Directory");
        if (denied is not null) return new JsonResult(new { success = false, message = "Access denied." }) { StatusCode = 403 };
        return new JsonResult(await StudentsService.SaveTrio(model, CurrentUserId));
    }
}
