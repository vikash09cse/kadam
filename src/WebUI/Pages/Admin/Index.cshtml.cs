using Core.DTOs.App;
using Core.Features.Admin;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages.Admin
{
    public class IndexModel(StudentService studentService, AuthenticationService authenticationService) : PageModel
    {
        public int ActiveCount { get; private set; }
        public int InactiveCount { get; private set; }
        public int CompletedCount { get; private set; }

        public async Task OnGetAsync()
        {
            var userId = authenticationService.GetCurrentUserId();
            var response = await studentService.GetAdminDashboardCount(userId);
            if (response.Result is DashboardDTO counts)
            {
                ActiveCount = counts.ActiveCount;
                InactiveCount = counts.InactiveCount;
                CompletedCount = counts.CompletedCount;
            }
        }
    }
}
