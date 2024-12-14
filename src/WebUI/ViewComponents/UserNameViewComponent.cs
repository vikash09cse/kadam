using Core.DTOs.Users;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.ViewComponents
{
    public class UserNameViewComponent : ViewComponent
    {
        private readonly AuthenticationService _authenticationService;

        public UserNameViewComponent(AuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public IViewComponentResult Invoke()
        {
            UserLoginValidateDTO user;
            try
            {
                user = _authenticationService.GetUserFromToken();
            }
            catch (Exception)
            {
                HttpContext.Response.Redirect("/login");
                return Content(string.Empty);
            }

            return View(user);
        }
    }
}
