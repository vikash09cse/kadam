using Core.DTOs;
using Core.Features.Admin;
using Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages.Admin
{
    public class UserMenuPermissionsModel(AdminService adminService, AuthenticationService authenticationService) : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int UserId { get; set; }

        public string UserFullName { get; set; } = string.Empty;
        public List<UserMenuPermissionItemDTO> Menus { get; set; } = [];

        [BindProperty]
        public List<int> SelectedMenuIds { get; set; } = [];

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (UserId <= 0)
            {
                return RedirectToPage("/Admin/Peoples");
            }

            await LoadPageAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (UserId <= 0)
            {
                return RedirectToPage("/Admin/Peoples");
            }

            var response = await adminService.SaveUserMenuPermissions(new UserMenuPermissionsDTO
            {
                UserId = UserId,
                MenuIds = SelectedMenuIds ?? []
            }, authenticationService.GetCurrentUserId());

            if (response.Success)
            {
                SuccessMessage = response.Message;
            }
            else
            {
                ErrorMessage = response.Message;
            }

            await LoadPageAsync();
            return Page();
        }

        private async Task LoadPageAsync()
        {
            await adminService.EnsureNavigationMenusSeeded();

            var user = await adminService.GetUser(UserId);
            UserFullName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "User";
            Menus = (await adminService.GetUserMenuPermissions(UserId)).ToList();
            SelectedMenuIds = Menus.Where(x => x.IsSelected).Select(x => x.Id).ToList();
        }
    }
}
