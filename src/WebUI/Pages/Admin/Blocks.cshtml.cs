using Core.Entities;
using Core.Features.Admin;
using Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages.Admin
{
    public class BlocksModel(AdminService adminService, AuthenticationService authenticationService) : PageModel
    {
        public async Task<IActionResult> OnGetBlockList(int draw, int start, int length, string searchValue)
        {
            int pageNumber = (start / length) + 1;
            var result = await adminService.GetBlocks(draw, pageNumber, length, searchValue);
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnPostSaveBlock([FromBody] Block block)
        {
            if (block == null)
            {
                return new JsonResult(new { success = false, message = MessageError.InvalidData });
            }
            
            var response = await adminService.SaveBlock(block, authenticationService.GetCurrentUserId());
            return new JsonResult(response);
        }

        public async Task<IActionResult> OnPostDeleteBlock(int id)
        {
            var response = await adminService.DeleteBlock(id, authenticationService.GetCurrentUserId());
            return new JsonResult(response);
        }

        public async Task<IActionResult> OnGetBlockDetail(int id)
        {
            var response = await adminService.GetBlock(id);
            return new JsonResult(response);
        }

        public async Task<IActionResult> OnGetDistrictListByState(int stateId)
        {
           var districts = await adminService.GetDistrictsByState(stateId);
           return new JsonResult(districts);
        }

        public async Task<IActionResult> OnGetStateList()
        {
            var states = await adminService.GetStatesByStatus(Enums.Status.Active); // Assuming this method returns a list of states
            return new JsonResult(states);
        }
    }
}
