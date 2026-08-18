using Core.Features.StudentsWeb;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.StudentPortal.Pages;

public sealed class DashboardModel(
    StudentsWebService studentsService,
    AuthenticationService authenticationService)
    : StudentPortalPageModel(studentsService, authenticationService)
{
    public StudentsWebDashboardDTO Dashboard { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var denied = await RequirePageAsync("/StudentPortal/Dashboard");
        if (denied is not null) return denied;
        Dashboard = await StudentsService.GetDashboard(CurrentUserId);
        return Page();
    }
}
