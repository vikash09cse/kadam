using Core.DTOs;
using Core.Features.Admin;
using Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebUI.Services;

namespace WebUI.Pages.Admin.Kadam
{
    public class ThemesModel : PageModel
    {
        private readonly AdminService _adminService;
        private readonly AuthenticationService _authenticationService;
        private readonly PagePermissionGuard _pagePermissions;

        public ThemesModel(AdminService adminService, AuthenticationService authenticationService, PagePermissionGuard pagePermissions)
        {
            _adminService = adminService;
            _authenticationService = authenticationService;
            _pagePermissions = pagePermissions;
        }

        public async Task<IActionResult> OnGetThemeList(int draw, int start, int length, string searchValue)
        {
            var result = await _adminService.GetThemes(draw, start, length, searchValue);
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnPostSaveTheme([FromBody] Core.Entities.Theme theme)
        {
            var denied = await _pagePermissions.ForbidAddEditAsync();
            if (denied != null) return denied;
            if (theme == null)
            {
                return new JsonResult(new { success = false, message = MessageError.InvalidData });
            }
            
            var response = await _adminService.SaveTheme(theme, _authenticationService.GetCurrentUserId());
            return new JsonResult(response);
        }

        public async Task<IActionResult> OnPostDeleteTheme(int id)
        {
            var denied = await _pagePermissions.ForbidDeleteAsync();
            if (denied != null) return denied;
            var response = await _adminService.DeleteTheme(id, _authenticationService.GetCurrentUserId());
            return new JsonResult(response);
        }

        public async Task<IActionResult> OnGetThemeDetail(int id)
        {
            var response = await _adminService.GetTheme(id);
            return new JsonResult(response);
        }
    }
}
