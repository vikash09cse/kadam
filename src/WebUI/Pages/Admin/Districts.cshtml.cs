using Core.Entities;
using Core.Features.Admin;
using Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages.Admin
{
    public class DistrictsModel(AdminService adminService, AuthenticationService authenticationService) : PageModel
    {
        public async Task<IActionResult> OnGetDistrictList(int draw, int start, int length, string searchValue)
        {
            int pageNumber = (start / length) + 1;
            var result = await adminService.GetDistricts(draw, pageNumber, length, searchValue);
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnPostSaveDistrict([FromBody] District district)
        {
            if (district == null)
            {
                return new JsonResult(new { success = false, message = MessageError.InvalidData });
            }
            
            var response = await adminService.SaveDistrict(district, authenticationService.GetCurrentUserId());
            return new JsonResult(response);
        }

        public async Task<IActionResult> OnPostDeleteDistrict(int id)
        {
            var response = await adminService.DeleteDistrict(id, authenticationService.GetCurrentUserId());
            return new JsonResult(response);
        }

        public async Task<IActionResult> OnGetDistrictDetail(int id)
        {
            var response = await adminService.GetDistrict(id);
            return new JsonResult(response);
        }

        public async Task<IActionResult> OnGetStateList()
        {
           var states = await adminService.GetStatesByStatus(Enums.Status.Active); // Assuming this method returns a list of states
           return new JsonResult(states);
        }
    }
}
