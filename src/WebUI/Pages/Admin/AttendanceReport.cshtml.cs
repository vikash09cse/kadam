using ClosedXML.Excel;
using Core.DTOs;
using Core.DTOs.App;
using Core.Features.Admin;
using Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace WebUI.Pages.Admin
{
    public class AttendanceReportModel(
        StudentService studentService,
        InstitutionService institutionService,
        AuthenticationService authenticationService) : PageModel
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        [BindProperty]
        public int InstitutionId { get; set; }

        [BindProperty]
        public int? GradeId { get; set; }

        [BindProperty]
        public string? Section { get; set; }

        [BindProperty]
        public DateTime FromDate { get; set; }

        [BindProperty]
        public DateTime ToDate { get; set; }

        public bool IsAdmin { get; private set; }
        public IEnumerable<DropdownDTO> Institutions { get; private set; } = [];
        public IEnumerable<DropdownDTO> Grades { get; private set; } = [];
        public IEnumerable<string> SectionOptions { get; private set; } = [];
        public IReadOnlyList<StudentAttendanceSummaryReportDTO> ReportRows { get; private set; } = [];
        public bool HasSearched { get; private set; }

        /// <summary>Grade/section map for client cascade (non-admin assigned data; admin loads via handler).</summary>
        public string GradeSectionsJson { get; private set; } = "[]";

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = authenticationService.GetCurrentUserId();
            if (userId <= 0)
            {
                return RedirectToPage("/Login");
            }

            SetDefaultDateRange();
            await LoadFiltersAsync(userId);
            return Page();
        }

        public async Task<IActionResult> OnPostSearchAsync()
        {
            var userId = authenticationService.GetCurrentUserId();
            if (userId <= 0)
            {
                return RedirectToPage("/Login");
            }

            if (!ValidateFilters())
            {
                await LoadFiltersAsync(userId);
                return Page();
            }

            await LoadReportAsync(userId);
            await LoadFiltersAsync(userId);
            return Page();
        }

        public async Task<IActionResult> OnPostDownloadExcelAsync()
        {
            var userId = authenticationService.GetCurrentUserId();
            if (userId <= 0)
            {
                return RedirectToPage("/Login");
            }

            if (!ValidateFilters())
            {
                await LoadFiltersAsync(userId);
                return Page();
            }

            try
            {
                var filter = BuildFilter();
                var data = (await studentService.GetStudentAttendanceSummaryReport(userId, filter)).ToList();

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Attendance Summary");

                var columns = GetExcelColumns();
                for (int col = 1; col <= columns.Count; col++)
                {
                    worksheet.Cell(1, col).Value = columns[col - 1].Header;
                }

                worksheet.Row(1).Style.Font.Bold = true;
                worksheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;

                int row = 2;
                foreach (var item in data)
                {
                    for (int col = 1; col <= columns.Count; col++)
                    {
                        worksheet.Cell(row, col).Value = columns[col - 1].Getter(item) ?? string.Empty;
                    }
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream, false);
                stream.Position = 0;

                var fileName = $"Student_Attendance_Summary_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(
                    stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex)
            {
                await LoadFiltersAsync(userId);
                ModelState.AddModelError(string.Empty, $"Unable to generate Excel. {ex.Message}");
                return Page();
            }
        }

        public async Task<IActionResult> OnGetGradeSectionsAsync(int institutionId)
        {
            var userId = authenticationService.GetCurrentUserId();
            if (userId <= 0)
            {
                return new JsonResult(Array.Empty<object>());
            }

            IsAdmin = await studentService.IsAdminUser(userId);
            IEnumerable<AppGradeSectionDTO> gradeSections;

            if (IsAdmin)
            {
                gradeSections = await studentService.GetGradeSectionsByInstitutionId(institutionId);
            }
            else
            {
                var institutions = await studentService.GetInstitutionsByUserId(userId);
                gradeSections = institutions
                    .FirstOrDefault(x => x.Id == institutionId)?
                    .GradeSections
                    ?? [];
            }

            var payload = gradeSections.Select(g => new
            {
                id = g.Id,
                gradeName = g.GradeName,
                sections = SplitSections(g.Sections)
            });

            return new JsonResult(payload);
        }

        private async Task LoadReportAsync(int userId)
        {
            var filter = BuildFilter();
            ReportRows = (await studentService.GetStudentAttendanceSummaryReport(userId, filter)).ToList();
            HasSearched = true;
        }

        private StudentAttendanceSummaryReportFilterDTO BuildFilter() => new()
        {
            InstitutionId = InstitutionId,
            GradeId = GradeId > 0 ? GradeId : null,
            Section = string.IsNullOrWhiteSpace(Section) ? null : Section.Trim(),
            FromDate = FromDate.Date,
            ToDate = ToDate.Date
        };

        private bool ValidateFilters()
        {
            if (InstitutionId <= 0)
            {
                ModelState.AddModelError(string.Empty, "Please select an institution.");
            }

            if (FromDate == default || ToDate == default)
            {
                ModelState.AddModelError(string.Empty, "From Date and To Date are required.");
            }
            else if (FromDate > ToDate)
            {
                ModelState.AddModelError(string.Empty, "From Date cannot be later than To Date.");
            }

            return ModelState.IsValid;
        }

        private void SetDefaultDateRange()
        {
            var today = DateTime.Today;
            FromDate = new DateTime(today.Year, today.Month, 1);
            ToDate = FromDate.AddMonths(1).AddDays(-1);
        }

        private async Task LoadFiltersAsync(int userId)
        {
            IsAdmin = await studentService.IsAdminUser(userId);

            List<AppInstitutionDTO> assignedInstitutions = [];
            if (IsAdmin)
            {
                var institutions = await institutionService.GetInstitutions(
                    1,
                    10000,
                    Enums.Status.Active,
                    string.Empty);

                Institutions = institutions
                    .Select(x => new DropdownDTO { Value = x.Id, Text = x.InstitutionName })
                    .OrderBy(x => x.Text);

                if (InstitutionId > 0)
                {
                    assignedInstitutions =
                    [
                        new AppInstitutionDTO
                        {
                            Id = InstitutionId,
                            InstitutionName = Institutions.FirstOrDefault(x => x.Value == InstitutionId)?.Text ?? string.Empty,
                            GradeSections = await studentService.GetGradeSectionsByInstitutionId(InstitutionId)
                        }
                    ];
                }
            }
            else
            {
                assignedInstitutions = (await studentService.GetInstitutionsByUserId(userId)).ToList();
                Institutions = assignedInstitutions
                    .Select(x => new DropdownDTO { Value = x.Id, Text = x.InstitutionName })
                    .OrderBy(x => x.Text);
            }

            GradeSectionsJson = JsonSerializer.Serialize(
                assignedInstitutions.SelectMany(i => i.GradeSections.Select(g => new
                {
                    institutionId = i.Id,
                    id = g.Id,
                    gradeName = g.GradeName,
                    sections = SplitSections(g.Sections)
                })),
                JsonOptions);

            await BindGradeSectionDropdownsAsync(userId);
        }

        private async Task BindGradeSectionDropdownsAsync(int userId)
        {
            Grades = [];
            SectionOptions = [];

            if (InstitutionId <= 0)
            {
                return;
            }

            IEnumerable<AppGradeSectionDTO> gradeSections;
            if (IsAdmin)
            {
                gradeSections = await studentService.GetGradeSectionsByInstitutionId(InstitutionId);
            }
            else
            {
                var institutions = await studentService.GetInstitutionsByUserId(userId);
                gradeSections = institutions.FirstOrDefault(x => x.Id == InstitutionId)?.GradeSections ?? [];
            }

            Grades = gradeSections
                .Select(g => new DropdownDTO { Value = g.Id, Text = g.GradeName })
                .OrderBy(x => x.Text);

            if (GradeId > 0)
            {
                var selected = gradeSections.FirstOrDefault(g => g.Id == GradeId);
                SectionOptions = SplitSections(selected?.Sections);
            }
        }

        private static List<string> SplitSections(string? sections)
        {
            if (string.IsNullOrWhiteSpace(sections))
            {
                return [];
            }

            return sections
                .Split([',', ';', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s)
                .ToList();
        }

        private static List<(string Header, Func<StudentAttendanceSummaryReportDTO, string?> Getter)> GetExcelColumns() =>
        [
            ("Student Id", x => x.StudentId),
            ("Student Name", x => x.StudentName),
            ("Institution", x => x.InstitutionName),
            ("Grade", x => x.GradeName),
            ("Section", x => x.Section),
            ("Present", x => x.PresentCount.ToString()),
            ("Absent", x => x.AbsentCount.ToString()),
            ("Holiday", x => x.HolidayCount.ToString()),
            ("Working Days", x => x.WorkingDays.ToString()),
            ("Attendance %", x => x.AttendancePercent.ToString("0.##"))
        ];
    }
}
