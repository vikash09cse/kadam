using Core.DTOs;
using Core.Entities;
using Core.Features.Admin;
using Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebUI.Services;

namespace WebUI.Pages.Admin.Kadam
{
    public class StepsModel : PageModel
    {
        private readonly AdminService _adminService;
        private readonly AuthenticationService _authenticationService;
        private readonly PagePermissionGuard _pagePermissions;

        public StepsModel(AdminService adminService, AuthenticationService authenticationService, PagePermissionGuard pagePermissions)
        {
            _adminService = adminService;
            _authenticationService = authenticationService;
            _pagePermissions = pagePermissions;
        }

        public async Task<IActionResult> OnGetStepList(int draw, int start, int length, string searchValue)
        {
            var result = await _adminService.GetSteps(draw, start, length, searchValue);
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnPostSaveStep([FromBody] Step step)
        {
            var denied = await _pagePermissions.ForbidAddEditAsync();
            if (denied != null) return denied;
            if (step == null)
            {
                return new JsonResult(new { success = false, message = MessageError.InvalidData });
            }
            
            var response = await _adminService.SaveStep(step, _authenticationService.GetCurrentUserId());
            return new JsonResult(response);
        }

        public async Task<IActionResult> OnPostDeleteStep(int id)
        {
            var denied = await _pagePermissions.ForbidDeleteAsync();
            if (denied != null) return denied;
            var response = await _adminService.DeleteStep(id, _authenticationService.GetCurrentUserId());
            return new JsonResult(response);
        }

        public async Task<IActionResult> OnGetStepDetail(int id)
        {
            var response = await _adminService.GetStep(id);
            return new JsonResult(response);
        }
    }
}
