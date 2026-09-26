using System.Text.Json;
using ClosedXML.Excel;
using Core.DTOs.Users;
using Core.Features.Admin;
using Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebUI.Services;

namespace WebUI.Pages.Admin
{
    public class PeopleBulkImportModel(
        AdminService adminService,
        AuthenticationService authenticationService,
        PagePermissionGuard pagePermissions) : PageModel
    {
        private const string CredentialsTempDataKey = "PeopleBulkImportCredentials";

        public List<PeopleImportErrorDTO> ImportErrors { get; set; } = [];
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }
        public bool HasCredentials { get; set; }

        public IActionResult OnGet()
        {
            HasCredentials = TempData.Peek(CredentialsTempDataKey) is string;
            return Page();
        }

        public IActionResult OnGetDownloadTemplate()
        {
            using var workbook = new XLWorkbook();

            var sheet = workbook.Worksheets.Add("People");
            var headers = new[]
            {
                "FirstName", "LastName", "Email", "UserName", "Phone",
                "AlternatePhone", "Gender", "Role", "ReporteeRole"
            };

            for (int col = 1; col <= headers.Length; col++)
            {
                sheet.Cell(1, col).Value = headers[col - 1];
            }

            sheet.Row(1).Style.Font.Bold = true;
            sheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;

            sheet.Cell(2, 1).Value = "John";
            sheet.Cell(2, 2).Value = "Doe";
            sheet.Cell(2, 3).Value = "john.doe@example.com";
            sheet.Cell(2, 4).Value = "johndoe";
            sheet.Cell(2, 5).Value = "9876543210";
            sheet.Cell(2, 6).Value = "";
            sheet.Cell(2, 7).Value = "Male";
            sheet.Cell(2, 8).Value = "Admin";
            sheet.Cell(2, 9).Value = "Admin";

            var instructions = workbook.Worksheets.Add("Instructions");
            instructions.Cell(1, 1).Value = "People Bulk Import Instructions";
            instructions.Cell(1, 1).Style.Font.Bold = true;
            instructions.Cell(3, 1).Value = "Required columns: FirstName, LastName, Email, UserName, Phone, Gender, Role, ReporteeRole";
            instructions.Cell(4, 1).Value = "Optional: AlternatePhone";
            instructions.Cell(5, 1).Value = "Gender values: Male, Female";
            instructions.Cell(6, 1).Value = "Role and ReporteeRole must match existing role names exactly.";
            instructions.Cell(7, 1).Value = "Email and UserName must be unique in the file and must not already exist.";
            instructions.Cell(8, 1).Value = "Passwords are auto-generated (6–15 characters; uppercase, lowercase, number, special @$!%*?&).";
            instructions.Cell(9, 1).Value = "Divisions and institutions are not imported; assign them later from Peoples.";
            instructions.Columns().AdjustToContents();
            sheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream, false);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "People_Bulk_Import_Template.xlsx");
        }

        public IActionResult OnGetDownloadCredentials()
        {
            if (TempData[CredentialsTempDataKey] is not string json
                || string.IsNullOrWhiteSpace(json))
            {
                ErrorMessage = "Credentials file is no longer available. Please run the import again.";
                return Page();
            }

            var credentials = JsonSerializer.Deserialize<List<PeopleImportCredentialDTO>>(json) ?? [];
            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Credentials");
            sheet.Cell(1, 1).Value = "UserName";
            sheet.Cell(1, 2).Value = "Email";
            sheet.Cell(1, 3).Value = "GeneratedPassword";
            sheet.Row(1).Style.Font.Bold = true;
            sheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;

            var row = 2;
            foreach (var item in credentials)
            {
                sheet.Cell(row, 1).Value = item.UserName;
                sheet.Cell(row, 2).Value = item.Email;
                sheet.Cell(row, 3).Value = item.GeneratedPassword;
                row++;
            }

            sheet.Columns().AdjustToContents();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream, false);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"People_Import_Credentials_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }

        public async Task<IActionResult> OnPostImportAsync(IFormFile excelFile)
        {
            if (!await pagePermissions.CanAddEditAsync())
            {
                ErrorMessage = MessageError.NoPermission;
                return Page();
            }

            if (excelFile == null || excelFile.Length == 0)
            {
                ErrorMessage = "Please select a valid Excel file.";
                return Page();
            }

            var extension = Path.GetExtension(excelFile.FileName).ToLowerInvariant();
            if (extension != ".xlsx" && extension != ".xls")
            {
                ErrorMessage = "Please upload a valid Excel file (.xlsx or .xls).";
                return Page();
            }

            var rows = ParseExcelRows(excelFile);
            if (rows.Count == 0)
            {
                ErrorMessage = "No data rows found in the file.";
                return Page();
            }

            try
            {
                var response = await adminService.BulkImportPeople(rows, authenticationService.GetCurrentUserId());
                if (response.Success && response.Result is PeopleImportResultDTO importResult)
                {
                    SuccessMessage = response.Message;
                    ImportErrors = [];
                    if (importResult.Credentials.Count > 0)
                    {
                        TempData[CredentialsTempDataKey] = JsonSerializer.Serialize(importResult.Credentials);
                        HasCredentials = true;
                    }
                }
                else if (response.Result is IEnumerable<PeopleImportErrorDTO> errors)
                {
                    ImportErrors = errors.ToList();
                    ErrorMessage = response.Message;
                    TempData.Remove(CredentialsTempDataKey);
                }
                else
                {
                    ErrorMessage = response.Message;
                    TempData.Remove(CredentialsTempDataKey);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Import failed: {ex.Message}";
                TempData.Remove(CredentialsTempDataKey);
            }

            return Page();
        }

        private static List<PeopleImportRowDTO> ParseExcelRows(IFormFile excelFile)
        {
            var rows = new List<PeopleImportRowDTO>();
            using var stream = excelFile.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;

            for (int row = 2; row <= lastRow; row++)
            {
                var importRow = new PeopleImportRowDTO
                {
                    RowNumber = row,
                    FirstName = worksheet.Cell(row, 1).GetValue<string>().Trim(),
                    LastName = worksheet.Cell(row, 2).GetValue<string>().Trim(),
                    Email = worksheet.Cell(row, 3).GetValue<string>().Trim(),
                    UserName = worksheet.Cell(row, 4).GetValue<string>().Trim(),
                    Phone = worksheet.Cell(row, 5).GetValue<string>().Trim(),
                    AlternatePhone = worksheet.Cell(row, 6).GetValue<string>().Trim(),
                    Gender = worksheet.Cell(row, 7).GetValue<string>().Trim(),
                    Role = worksheet.Cell(row, 8).GetValue<string>().Trim(),
                    ReporteeRole = worksheet.Cell(row, 9).GetValue<string>().Trim()
                };

                if (IsEmptyRow(importRow))
                    continue;

                rows.Add(importRow);
            }

            return rows;
        }

        private static bool IsEmptyRow(PeopleImportRowDTO row)
        {
            return string.IsNullOrWhiteSpace(row.FirstName)
                && string.IsNullOrWhiteSpace(row.LastName)
                && string.IsNullOrWhiteSpace(row.Email)
                && string.IsNullOrWhiteSpace(row.UserName)
                && string.IsNullOrWhiteSpace(row.Phone)
                && string.IsNullOrWhiteSpace(row.Role);
        }
    }
}
