using ClosedXML.Excel;
using Core.DTOs;
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
            var result = await adminService.GetDistricts(draw, start, length, searchValue);
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
           var states = await adminService.GetStatesByStatus(Enums.Status.Active);
           return new JsonResult(states);
        }

        public async Task<IActionResult> OnPostBulkImportDistricts(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
                return new JsonResult(new { success = false, message = "Please select a valid Excel file." });

            var extension = Path.GetExtension(excelFile.FileName).ToLowerInvariant();
            if (extension != ".xlsx" && extension != ".xls")
                return new JsonResult(new { success = false, message = "Please upload a valid Excel file (.xlsx or .xls)." });

            var rows = new List<DistrictImportRowDTO>();
            using var stream = excelFile.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;

            for (int row = 2; row <= lastRow; row++)
            {
                var stateName    = worksheet.Cell(row, 1).GetValue<string>().Trim();
                var districtName = worksheet.Cell(row, 2).GetValue<string>().Trim();
                var districtCode = worksheet.Cell(row, 3).GetValue<string>().Trim();

                // Skip fully empty rows
                if (string.IsNullOrWhiteSpace(stateName) &&
                    string.IsNullOrWhiteSpace(districtName) &&
                    string.IsNullOrWhiteSpace(districtCode))
                    continue;

                rows.Add(new DistrictImportRowDTO
                {
                    RowNumber    = row,
                    StateName    = stateName,
                    DistrictName = districtName,
                    DistrictCode = districtCode
                });
            }

            if (rows.Count == 0)
                return new JsonResult(new { success = false, message = "No data rows found in the file." });

            var response = await adminService.BulkImportDistricts(rows, authenticationService.GetCurrentUserId());
            return new JsonResult(response);
        }
    }
}
