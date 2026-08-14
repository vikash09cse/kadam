using Core.Features.Admin;
using Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebUI.Services;

namespace WebUI.Pages.Admin
{
    public class ProgramsModel : PageModel
    {
        private readonly AdminService _adminService;
        private readonly AuthenticationService _authenticationService;
        private readonly PagePermissionGuard _pagePermissions;

        public ProgramsModel(AdminService adminService, AuthenticationService authenticationService, PagePermissionGuard pagePermissions)
        {
            _adminService = adminService;
            _authenticationService = authenticationService;
            _pagePermissions = pagePermissions;
        }

        public async Task<IActionResult> OnGetProgramList(int draw, int start, int length, string searchValue)
        {
            int pageNumber = (start / length) + 1;
            var result = await _adminService.GetPrograms(draw, pageNumber, length, searchValue);
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnPostSaveProgram([FromBody] Core.Entities.Program program)
        {
            var denied = await _pagePermissions.ForbidAddEditAsync();
            if (denied != null) return denied;
            if (program == null)
            {
                return new JsonResult(new { success = false, message = MessageError.InvalidData });
            }
            
            var response = await _adminService.SaveProgram(program, _authenticationService.GetCurrentUserId());
            return new JsonResult(response);
        }

        public async Task<IActionResult> OnPostDeleteProgram(int id)
        {
            var denied = await _pagePermissions.ForbidDeleteAsync();
            if (denied != null) return denied;
            var response = await _adminService.DeleteProgram(id, _authenticationService.GetCurrentUserId());
            return new JsonResult(response);
        }

        public async Task<IActionResult> OnGetProgramDetail(int id)
        {
            var response = await _adminService.GetProgram(id);
            return new JsonResult(response);
        }

        //public async Task<IActionResult> OnPostCloseProgram(int id)
        //{
        //    var response = await _adminService.CloseProgram(id, _authenticationService.GetCurrentUserId());
        //    return new JsonResult(response);
        //}
    }
}
