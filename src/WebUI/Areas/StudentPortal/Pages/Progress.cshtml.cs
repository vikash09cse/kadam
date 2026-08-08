using Core.Features.StudentsWeb;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.StudentPortal.Pages;

public sealed class ProgressModel(
    StudentsWebService studentsService,
    AuthenticationService authenticationService)
    : StudentPortalPageModel(studentsService, authenticationService)
{
    public StudentsWebProgressDTO Progress { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(int studentId)
    {
        var denied = await RequirePageAsync("/StudentPortal/MyInstitution");
        if (denied is not null) return denied;
        var progress = await StudentsService.GetProgress(studentId, CurrentUserId);
        if (progress is null)
        {
            TempData["ErrorMessage"] = "A completed baseline is required before progress.";
            return Redirect("/StudentPortal/MyInstitution");
        }
        Progress = progress;
        return Page();
    }

    public async Task<IActionResult> OnPostCompleteStepAsync(int studentId, int stepId)
    {
        var denied = await RequirePageAsync("/StudentPortal/MyInstitution");
        if (denied is not null) return denied;
        var result = await StudentsService.CompleteProgressStep(studentId, stepId, CurrentUserId);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return Redirect($"/StudentPortal/Progress?studentId={studentId}");
    }
}
