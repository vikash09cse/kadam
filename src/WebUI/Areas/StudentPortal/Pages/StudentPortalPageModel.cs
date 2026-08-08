using Core.Features.StudentsWeb;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Areas.StudentPortal.Pages;

public abstract class StudentPortalPageModel(
    StudentsWebService studentsService,
    AuthenticationService authenticationService) : PageModel
{
    protected StudentsWebService StudentsService { get; } = studentsService;
    protected int CurrentUserId => authenticationService.GetCurrentUserId();

    protected async Task<IActionResult?> RequirePageAsync(string pageUrl)
    {
        if (CurrentUserId <= 0)
            return Challenge();

        return await StudentsService.HasPageAccess(CurrentUserId, pageUrl)
            ? null
            : RedirectToPage("/AccessDenied");
    }
}
