using Core.Features.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly AdminService _userService;


        public IndexModel(ILogger<IndexModel> logger, AdminService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        public async Task OnGet()
        {
            var users = await _userService.GetUsers();
            foreach (var user in users)
            {

            }

        }
    }
}
