using Core.DTOs;
using Core.Features.Admin;
using Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages.Admin
{
    public class RolesModel : PageModel
    {
        private readonly AdminService _adminService;
        private readonly AuthenticationService _authenticationService;

        public RolesModel(AdminService adminService, AuthenticationService authenticationService)
        {
            _adminService = adminService;
            _authenticationService = authenticationService;
        }

        public async Task<IActionResult> OnGetRoleList(int draw, int start, int length, string searchValue)
        {
            var result = await _adminService.GetRoles(draw, start, length, searchValue);
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnPostSaveRole([FromBody] Core.Entities.Role role)
        {
            if (role == null)
            {
                return new JsonResult(new { success = false, message = MessageError.InvalidData });
            }
            
            var response = await _adminService.SaveRole(role, _authenticationService.GetCurrentUserId());
            return new JsonResult(response);
        }

        public async Task<IActionResult> OnPostDeleteRole(int id)
        {
            var response = await _adminService.DeleteRole(id, _authenticationService.GetCurrentUserId());
            return new JsonResult(response);
        }

        public async Task<IActionResult> OnGetRoleDetail(int id)
        {
            var response = await _adminService.GetRole(id);
            return new JsonResult(response);
        }

        public async Task<IActionResult> OnGetRolePermissions(int roleId)
        {
            var permissions = await _adminService.GetRolePermissions(roleId);
            return new JsonResult(permissions);
        }

        public async Task<IActionResult> OnPostSaveRolePermissions([FromBody] RolePermissionsDTO model)
        {
            if (model == null)
            {
                return new JsonResult(new { success = false, message = MessageError.InvalidData });
            }
            
            var response = await _adminService.SaveRolePermissions(model, _authenticationService.GetCurrentUserId());
            return new JsonResult(response);
        }
    }
}
