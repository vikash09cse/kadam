using Core.Entities;
using Core.Features.Admin;
using Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages.Admin
{
    public class DivisionsModel(AdminService adminService, AuthenticationService authenticationService) : PageModel
    {
        //public void OnGet()
        //{

        //}

        public async Task<IActionResult> OnGetDivisionList(int draw, int start, int length, string searchValue)
        {
            int pageNumber = (start / length) + 1;
            var result = await adminService.GetDivisions(draw, pageNumber, length, searchValue);
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnPostSaveDivision([FromBody] Division division)
        {
            if (division == null)
            {
                return new JsonResult(new { success = false, message = MessageError.InvalidData });
            }
            
            var response = await adminService.SaveDivision(division, authenticationService.GetCurrentUserId());
            return new JsonResult(response);
        }

        public async Task<IActionResult> OnPostDeleteDivision(int id)
        {
            var response = await adminService.DeleteDivision(id, authenticationService.GetCurrentUserId());
            return new JsonResult(response);
        }

        public async Task<IActionResult> OnGetDivisionDetail(int id)
        {
            var response = await adminService.GetDivision(id);
            return new JsonResult(response);
        }
    }
}
