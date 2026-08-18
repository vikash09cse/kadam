using Core.DTOs;
using Core.Features.Admin;
using Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebUI.Services;

namespace WebUI.Pages.Admin
{
    public class UserMenuPermissionsModel(AdminService adminService, AuthenticationService authenticationService, PagePermissionGuard pagePermissions) : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int UserId { get; set; }

        public string UserFullName { get; set; } = string.Empty;
        public List<UserMenuPermissionItemDTO> Menus { get; set; } = [];

        [BindProperty]
        public List<int> SelectedMenuIds { get; set; } = [];

        [BindProperty]
        public List<int> AddEditMenuIds { get; set; } = [];

        [BindProperty]
        public List<int> DeleteMenuIds { get; set; } = [];

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

            if (!await pagePermissions.CanAddEditAsync())
            {
                ErrorMessage = MessageError.NoPermission;
                await LoadPageAsync();
                return Page();
            }

            var selected = SelectedMenuIds ?? [];
            var addEdit = AddEditMenuIds ?? [];
            var delete = DeleteMenuIds ?? [];

            var response = await adminService.SaveUserMenuPermissions(new UserMenuPermissionsDTO
            {
                UserId = UserId,
                Permissions = selected.Distinct().Select(id => new UserMenuPermissionSaveItemDTO
                {
                    MenuId = id,
                    CanAddEdit = addEdit.Contains(id),
                    CanDelete = delete.Contains(id)
                }).ToList()
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
            AddEditMenuIds = Menus.Where(x => x.IsSelected && x.CanAddEdit).Select(x => x.Id).ToList();
            DeleteMenuIds = Menus.Where(x => x.IsSelected && x.CanDelete).Select(x => x.Id).ToList();
        }
    }
}
