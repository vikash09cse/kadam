using Core.DTOs;
using Core.Features.Admin;
using Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages.Admin.Kadam
{
    public class ThemesModel : PageModel
    {
        private readonly AdminService _adminService;
        private readonly AuthenticationService _authenticationService;

        public ThemesModel(AdminService adminService, AuthenticationService authenticationService)
        {
            _adminService = adminService;
            _authenticationService = authenticationService;
        }

        public async Task<IActionResult> OnGetThemeList(int draw, int start, int length, string searchValue)
        {
            var result = await _adminService.GetThemes(draw, start, length, searchValue);
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnPostSaveTheme([FromBody] Core.Entities.Theme theme)
        {
            if (theme == null)
            {
                return new JsonResult(new { success = false, message = MessageError.InvalidData });
            }
            
            var response = await _adminService.SaveTheme(theme, _authenticationService.GetCurrentUserId());
            return new JsonResult(response);
        }

        public async Task<IActionResult> OnPostDeleteTheme(int id)
        {
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
