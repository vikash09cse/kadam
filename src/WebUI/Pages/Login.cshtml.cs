using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Core.Features.Admin;
using Core.DTOs;
using Core.DTOs.Users;

namespace WebUI.Pages
{
    // [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly AdminService _adminService;
        private readonly AuthenticationService _authenticationService;

        public LoginModel(AdminService adminService, AuthenticationService authenticationService)
        {
            _adminService = adminService;
            _authenticationService = authenticationService;
        }

        [BindProperty]
        [Required(ErrorMessage = "User Id is required")]
        public required string Email { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [BindProperty]
        public bool RememberMe { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var redirectResult = await _authenticationService.CheckTokenAndRedirect();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var response = await _adminService.ValidateUser(Email, Password);
            if (response.StatusCode == AppStatusCodes.Success)
            {
                var userInfo = (UserLoginValidateDTO)response.Result;
                await _authenticationService.SignInUser(userInfo, RememberMe);
                return RedirectToPage("/Admin/Index");
            }

            ModelState.AddModelError(string.Empty, response.Message);
            return Page();
        }
    }
}
