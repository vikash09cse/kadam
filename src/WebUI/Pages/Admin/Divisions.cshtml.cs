using Core.DTOs;
using Core.Entities;
using Core.Features.Admin;
using Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages.Admin
{
    public class DivisionsModel(AdminService adminService, AuthenticationService authenticationService) : PageModel
    {
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

        public async Task<IActionResult> OnGetStateList()
        {
            var states = await adminService.GetStatesByStatus(Enums.Status.Active);
            return new JsonResult(states);
        }

        public async Task<IActionResult> OnGetDistrictListByState(int stateId)
        {
            var districts = await adminService.GetDistrictsByState(stateId);
            return new JsonResult(districts);
        }

        private static int[] ParseCommaSeparatedIds(string? ids)
        {
            if (string.IsNullOrWhiteSpace(ids))
            {
                return [];
            }

            return ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(value => int.TryParse(value.Trim(), out var id) ? id : 0)
                .Where(id => id > 0)
                .Distinct()
                .ToArray();
        }

        public async Task<IActionResult> OnGetBlockListByDistrictIds([FromQuery] string districtIds)
        {
            var blocks = await adminService.GetBlocksByDistrictIds(ParseCommaSeparatedIds(districtIds));
            return new JsonResult(blocks);
        }

        public async Task<IActionResult> OnGetVillageListByBlockIds([FromQuery] string blockIds)
        {
            var villages = await adminService.GetVillagesByBlockIds(ParseCommaSeparatedIds(blockIds));
            return new JsonResult(villages);
        }

        public async Task<IActionResult> OnGetDivisionLocation(int divisionId)
        {
            var assignment = await adminService.GetDivisionLocationAssignment(divisionId);
            return new JsonResult(assignment);
        }

        public async Task<IActionResult> OnPostSaveDivisionLocation([FromBody] SaveDivisionLocationDTO assignment)
        {
            if (assignment == null)
            {
                return new JsonResult(new { success = false, message = MessageError.InvalidData });
            }

            var response = await adminService.SaveDivisionLocation(assignment, authenticationService.GetCurrentUserId());
            return new JsonResult(response);
        }
    }
}
