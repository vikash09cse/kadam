using Core.Features.Admin;
using Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages.Admin
{
    public class MenuPermissionsModel : PageModel
    {
        private readonly AdminService _adminService;
        private readonly AuthenticationService _authenticationService;

        public MenuPermissionsModel(AdminService adminService, AuthenticationService authenticationService)
        {
            _adminService = adminService;
            _authenticationService = authenticationService;
        }

        public async Task<IActionResult> OnGetMenuPermissionList(int draw, int start, int length, string searchValue)
        {
            int pageNumber = (start / length) + 1;
            var result = await _adminService.GetMenuPermissions(draw, pageNumber, length, searchValue);
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnGetParentMenus()
        {
            var result = await _adminService.GetMenus();
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnGetMenuPermissionDetail(int id)
        {
            var response = await _adminService.GetMenuPermission(id);
            return new JsonResult(response);
        }

        public async Task<IActionResult> OnPostSaveMenuPermission([FromBody] Core.Entities.MenuPermission menuPermission)
        {
            if (menuPermission == null)
            {
                return new JsonResult(new { success = false, message = MessageError.InvalidData });
            }
            
            var response = await _adminService.SaveMenuPermission(menuPermission, _authenticationService.GetCurrentUserId());
            return new JsonResult(response);
        }

        public async Task<IActionResult> OnPostDeleteMenuPermission(int id)
        {
            var response = await _adminService.DeleteMenuPermission(id, _authenticationService.GetCurrentUserId());
            return new JsonResult(response);
        }
    }
}
