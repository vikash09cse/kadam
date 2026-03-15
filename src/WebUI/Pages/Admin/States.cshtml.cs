using ClosedXML.Excel;
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
            var result = await adminService.GetStates(draw, start, length, searchValue);
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

        public async Task<IActionResult> OnPostBulkImportStates(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
                return new JsonResult(new { success = false, message = "Please select a valid Excel file." });

            var extension = Path.GetExtension(excelFile.FileName).ToLowerInvariant();
            if (extension != ".xlsx" && extension != ".xls")
                return new JsonResult(new { success = false, message = "Please upload a valid Excel file (.xlsx or .xls)." });

            var stateRows = new List<(string Name, string Code)>();
            using var stream = excelFile.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;

            for (int row = 2; row <= lastRow; row++)
            {
                var name = worksheet.Cell(row, 1).GetValue<string>();
                var code = worksheet.Cell(row, 2).GetValue<string>();
                stateRows.Add((name, code));
            }

            var response = await adminService.BulkImportStates(stateRows, authenticationService.GetCurrentUserId());
            return new JsonResult(response);
        }
    }
}
