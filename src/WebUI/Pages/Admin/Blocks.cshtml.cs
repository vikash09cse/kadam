using ClosedXML.Excel;
using Core.DTOs;
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
            var result = await adminService.GetBlocks(draw, start, length, searchValue);
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
            var states = await adminService.GetStatesByStatus(Enums.Status.Active);
            return new JsonResult(states);
        }

        public async Task<IActionResult> OnPostBulkImportBlocks(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
                return new JsonResult(new { success = false, message = "Please select a valid Excel file." });

            var extension = Path.GetExtension(excelFile.FileName).ToLowerInvariant();
            if (extension != ".xlsx" && extension != ".xls")
                return new JsonResult(new { success = false, message = "Please upload a valid Excel file (.xlsx or .xls)." });

            var rows = new List<BlockImportRowDTO>();
            using var stream = excelFile.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;

            for (int row = 2; row <= lastRow; row++)
            {
                var stateName    = worksheet.Cell(row, 1).GetValue<string>().Trim();
                var districtName = worksheet.Cell(row, 2).GetValue<string>().Trim();
                var blockName    = worksheet.Cell(row, 3).GetValue<string>().Trim();
                // Column D (Block Code) is read but not stored — Block entity has no BlockCode field

                // Skip fully empty rows
                if (string.IsNullOrWhiteSpace(stateName) &&
                    string.IsNullOrWhiteSpace(districtName) &&
                    string.IsNullOrWhiteSpace(blockName))
                    continue;

                rows.Add(new BlockImportRowDTO
                {
                    RowNumber    = row,
                    StateName    = stateName,
                    DistrictName = districtName,
                    BlockName    = blockName
                });
            }

            if (rows.Count == 0)
                return new JsonResult(new { success = false, message = "No data rows found in the file." });

            var response = await adminService.BulkImportBlocks(rows, authenticationService.GetCurrentUserId());
            return new JsonResult(response);
        }
    }
}
