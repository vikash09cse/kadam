using Core.Entities;
using Core.Features.Admin;
using Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages.Admin
{
    public class StatesModel(AdminService adminService, AuthenticationService authenticationService) : PageModel
    {
        public async Task<IActionResult> OnGetStateList(int draw, int start, int length, string searchValue)
        {
            int pageNumber = (start / length) + 1;
            var result = await adminService.GetStates(draw, pageNumber, length, searchValue);
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnPostSaveState([FromBody] State state)
        {
            if (state == null)
            {
                return new JsonResult(new { success = false, message = MessageError.InvalidData });
            }
            
            var response = await adminService.SaveState(state, authenticationService.GetCurrentUserId());
            return new JsonResult(response);
        }

        public async Task<IActionResult> OnPostDeleteState(int id)
        {
            var response = await adminService.DeleteState(id, authenticationService.GetCurrentUserId());
            return new JsonResult(response);
        }

        public async Task<IActionResult> OnGetStateDetail(int id)
        {
            var response = await adminService.GetState(id);
            return new JsonResult(response);
        }

        public async Task<IActionResult> OnPostCloseState(int id)
        {
            var response = await adminService.CloseState(id, authenticationService.GetCurrentUserId());
            return new JsonResult(response);
        }
    }
}
