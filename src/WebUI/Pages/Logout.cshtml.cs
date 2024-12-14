using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly AuthenticationService _authenticationService;

        public LogoutModel(AuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public void OnGet()
        {
            _authenticationService.Logout();
            Response.Redirect("/Login");
        }
    }
}
