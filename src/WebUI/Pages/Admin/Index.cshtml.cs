using Core.Features.Admin;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly AdminService _adminService;
        public IndexModel(AdminService adminService)
        {
            _adminService = adminService;
        }

        public void OnGet()
        {
            // Get all the users
            var users = _adminService.GetUsers();

        }
    }
}
