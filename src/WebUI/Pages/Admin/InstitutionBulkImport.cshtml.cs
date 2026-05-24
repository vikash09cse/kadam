using ClosedXML.Excel;
using Core.DTOs;
using Core.Features.Admin;
using Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages.Admin
{
    public class InstitutionBulkImportModel(InstitutionService institutionService, AuthenticationService authenticationService) : PageModel
    {
        public List<InstitutionImportErrorDTO> ImportErrors { get; set; } = [];
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            return Page();
        }

        public IActionResult OnGetDownloadTemplate()
        {
            using var workbook = new XLWorkbook();

            var sheet = workbook.Worksheets.Add("Institutions");
            var headers = new[]
            {
                "Division", "State", "District", "Block", "Village",
                "Institution Type", "Institution Building",
                "Institution Name", "Institution Code", "Institution Id",
                "Head Master / Principal", "Contact",
                "Male Teachers", "Female Teachers", "Total Students",
                "Financial Year Start", "Financial Year End", "Grade Sections"
            };

            for (int col = 1; col <= headers.Length; col++)
            {
                sheet.Cell(1, col).Value = headers[col - 1];
            }

            sheet.Row(1).Style.Font.Bold = true;
            sheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;

            sheet.Cell(2, 1).Value = "Sample Division";
            sheet.Cell(2, 2).Value = "Sample State";
            sheet.Cell(2, 3).Value = "Sample District";
            sheet.Cell(2, 4).Value = "Sample Block";
            sheet.Cell(2, 5).Value = "Sample Village";
            sheet.Cell(2, 6).Value = "Primary School";
            sheet.Cell(2, 7).Value = "Govt. School";
            sheet.Cell(2, 8).Value = "Sample Institution";
            sheet.Cell(2, 9).Value = "INST-001";
            sheet.Cell(2, 10).Value = "ID-001";
            sheet.Cell(2, 11).Value = "John Doe";
            sheet.Cell(2, 12).Value = "9876543210";
            sheet.Cell(2, 13).Value = "5";
            sheet.Cell(2, 14).Value = "3";
            sheet.Cell(2, 15).Value = "120";
            sheet.Cell(2, 16).Value = "2025-04-01";
            sheet.Cell(2, 17).Value = "2026-03-31";
            sheet.Cell(2, 18).Value = "1st:A,B|2nd:A";

            var instructions = workbook.Worksheets.Add("Instructions");
            instructions.Cell(1, 1).Value = "Institution Type values";
            instructions.Cell(2, 1).Value = "Primary School, Middle School, High School, Pre School, DIET";
            instructions.Cell(4, 1).Value = "Institution Building values";
            instructions.Cell(5, 1).Value = "Public, Private, Govt. School";
            instructions.Cell(7, 1).Value = "Grade Sections format";
            instructions.Cell(8, 1).Value = "Use Grade:Section1,Section2|Grade2:Section1 (Example: 1st:A,B|2nd:A|Kadam STC:NA)";
            instructions.Cell(10, 1).Value = "Location names must match existing Division, State, District, Block, and Village records.";
            instructions.Columns().AdjustToContents();

            sheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream, false);
            stream.Position = 0;
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Institution_Bulk_Import_Template.xlsx");
        }

        public async Task<IActionResult> OnPostImportAsync(IFormFile excelFile)
        {
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
                var response = await institutionService.BulkImportInstitutions(rows, authenticationService.GetCurrentUserId());
                if (response.Success)
                {
                    SuccessMessage = response.Message;
                    ImportErrors = [];
                }
                else if (response.Result is IEnumerable<InstitutionImportErrorDTO> errors)
                {
                    ImportErrors = errors.ToList();
                    ErrorMessage = response.Message;
                }
                else
                {
                    ErrorMessage = response.Message;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Import failed: {ex.Message}";
            }

            return Page();
        }

        private static List<InstitutionImportRowDTO> ParseExcelRows(IFormFile excelFile)
        {
            var rows = new List<InstitutionImportRowDTO>();
            using var stream = excelFile.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;

            for (int row = 2; row <= lastRow; row++)
            {
                var importRow = new InstitutionImportRowDTO
                {
                    RowNumber = row,
                    DivisionName = worksheet.Cell(row, 1).GetValue<string>().Trim(),
                    StateName = worksheet.Cell(row, 2).GetValue<string>().Trim(),
                    DistrictName = worksheet.Cell(row, 3).GetValue<string>().Trim(),
                    BlockName = worksheet.Cell(row, 4).GetValue<string>().Trim(),
                    VillageName = worksheet.Cell(row, 5).GetValue<string>().Trim(),
                    InstitutionTypeName = worksheet.Cell(row, 6).GetValue<string>().Trim(),
                    InstitutionBuildingName = worksheet.Cell(row, 7).GetValue<string>().Trim(),
                    InstitutionName = worksheet.Cell(row, 8).GetValue<string>().Trim(),
                    InstitutionCode = worksheet.Cell(row, 9).GetValue<string>().Trim(),
                    InstitutionBusinessId = worksheet.Cell(row, 10).GetValue<string>().Trim(),
                    InstitutionHeadMasterName = worksheet.Cell(row, 11).GetValue<string>().Trim(),
                    InstitutionPhone = worksheet.Cell(row, 12).GetValue<string>().Trim(),
                    MaleTeachers = worksheet.Cell(row, 13).GetValue<string>().Trim(),
                    FemaleTeachers = worksheet.Cell(row, 14).GetValue<string>().Trim(),
                    TotalStudents = worksheet.Cell(row, 15).GetValue<string>().Trim(),
                    FinancialYearStart = GetCellDateOrText(worksheet.Cell(row, 16)),
                    FinancialYearEnd = GetCellDateOrText(worksheet.Cell(row, 17)),
                    GradeSections = worksheet.Cell(row, 18).GetValue<string>().Trim()
                };

                if (IsEmptyRow(importRow))
                {
                    continue;
                }

                rows.Add(importRow);
            }

            return rows;
        }

        private static bool IsEmptyRow(InstitutionImportRowDTO row)
        {
            return string.IsNullOrWhiteSpace(row.DivisionName)
                && string.IsNullOrWhiteSpace(row.StateName)
                && string.IsNullOrWhiteSpace(row.DistrictName)
                && string.IsNullOrWhiteSpace(row.BlockName)
                && string.IsNullOrWhiteSpace(row.VillageName)
                && string.IsNullOrWhiteSpace(row.InstitutionName);
        }

        private static string GetCellDateOrText(IXLCell cell)
        {
            if (cell.DataType == XLDataType.DateTime)
            {
                return cell.GetDateTime().ToString("yyyy-MM-dd");
            }

            return cell.GetValue<string>().Trim();
        }
    }
}
