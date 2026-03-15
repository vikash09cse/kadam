using ClosedXML.Excel;
using Core.DTOs;
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
            var result = await adminService.GetVillages(draw, start, length, searchValue);
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

        public async Task<IActionResult> OnPostBulkImportVillages(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
                return new JsonResult(new { success = false, message = "Please select a valid Excel file." });

            var extension = Path.GetExtension(excelFile.FileName).ToLowerInvariant();
            if (extension != ".xlsx" && extension != ".xls")
                return new JsonResult(new { success = false, message = "Please upload a valid Excel file (.xlsx or .xls)." });

            var rows = new List<VillageImportRowDTO>();
            using var stream = excelFile.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;

            for (int row = 2; row <= lastRow; row++)
            {
                var stateName    = worksheet.Cell(row, 1).GetValue<string>().Trim();
                var districtName = worksheet.Cell(row, 2).GetValue<string>().Trim();
                var blockName    = worksheet.Cell(row, 3).GetValue<string>().Trim();
                var villageName  = worksheet.Cell(row, 4).GetValue<string>().Trim();

                // Skip fully empty rows
                if (string.IsNullOrWhiteSpace(stateName) &&
                    string.IsNullOrWhiteSpace(districtName) &&
                    string.IsNullOrWhiteSpace(blockName) &&
                    string.IsNullOrWhiteSpace(villageName))
                    continue;

                rows.Add(new VillageImportRowDTO
                {
                    RowNumber    = row,
                    StateName    = stateName,
                    DistrictName = districtName,
                    BlockName    = blockName,
                    VillageName  = villageName
                });
            }

            if (rows.Count == 0)
                return new JsonResult(new { success = false, message = "No data rows found in the file." });

            var response = await adminService.BulkImportVillages(rows, authenticationService.GetCurrentUserId());
            return new JsonResult(response);
        }
    }
}
