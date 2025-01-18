using Core.Entities;
using Core.Features.Admin;
using Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages.Admin
{
    public class VillagesModel(AdminService adminService, AuthenticationService authenticationService) : PageModel
    {
        public async Task<IActionResult> OnGetVillageList(int draw, int start, int length, string searchValue)
        {
            int pageNumber = (start / length) + 1;
            var result = await adminService.GetVillages(draw, pageNumber, length, searchValue);
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnPostSaveVillage([FromBody] Village village)
        {
            if (village == null)
            {
                return new JsonResult(new { success = false, message = MessageError.InvalidData });
            }
            
            var response = await adminService.SaveVillage(village, authenticationService.GetCurrentUserId());
            return new JsonResult(response);
        }

        public async Task<IActionResult> OnPostDeleteVillage(int id)
        {
            var response = await adminService.DeleteVillage(id, authenticationService.GetCurrentUserId());
            return new JsonResult(response);
        }

        public async Task<IActionResult> OnGetVillageDetail(int id)
        {
            var response = await adminService.GetVillage(id);
            return new JsonResult(response);
        }
        public async Task<IActionResult> OnGetStateList()
        {
            var states = await adminService.GetStatesByStatus(Enums.Status.Active); // Assuming this method returns a list of states
            return new JsonResult(states);
        }
        public async Task<IActionResult> OnGetDistrictListByState(int stateId)
        {
            var districts = await adminService.GetDistrictsByState(stateId);
            return new JsonResult(districts);
        }
        public async Task<IActionResult> OnGetBlockListByDistrict(int districtId)
        {
           var blocks = await adminService.GetBlocksByDistrict(districtId);
           return new JsonResult(blocks);
        }
    }
}
