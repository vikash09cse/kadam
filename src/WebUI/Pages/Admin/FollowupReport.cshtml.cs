using ClosedXML.Excel;
using Core.DTOs;
using Core.DTOs.App;
using Core.Features.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages.Admin
{
    public class FollowupReportModel(
        StudentFollowupService studentFollowupService,
        StudentService studentService,
        AdminService adminService,
        AuthenticationService authenticationService) : PageModel
    {
        [BindProperty]
        public int? StateId { get; set; }

        [BindProperty]
        public int? DivisionId { get; set; }

        [BindProperty]
        public int? InstitutionId { get; set; }

        [BindProperty]
        public int? GradeId { get; set; }

        [BindProperty]
        public string? Section { get; set; }

        [BindProperty]
        public DateTime FromDate { get; set; }

        [BindProperty]
        public DateTime ToDate { get; set; }

        public IEnumerable<DropdownDTO> States { get; private set; } = [];
        public IEnumerable<DropdownDTO> Divisions { get; private set; } = [];
        public IEnumerable<DropdownDTO> Institutions { get; private set; } = [];
        public IEnumerable<DropdownDTO> Grades { get; private set; } = [];
        public IEnumerable<string> SectionOptions { get; private set; } = [];
        public IReadOnlyList<StudentFollowupListDTO> ReportRows { get; private set; } = [];
        public bool HasSearched { get; private set; }

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

            if (!await ValidateFiltersAsync(userId))
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

            if (!await ValidateFiltersAsync(userId))
            {
                await LoadFiltersAsync(userId);
                return Page();
            }

            try
            {
                var data = (await studentFollowupService.GetFollowupReport(userId, BuildFilter())).ToList();

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Student Follow-up");

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

                var fileName = $"Student_Followup_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
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

        public async Task<IActionResult> OnGetDivisionsByState(int? stateId)
        {
            var userId = authenticationService.GetCurrentUserId();
            if (userId <= 0)
            {
                return Unauthorized();
            }

            var divisions = await adminService.GetDivisionsForUser(userId, stateId > 0 ? stateId : null);
            return new JsonResult(divisions);
        }

        public async Task<IActionResult> OnGetInstitutions(int? stateId, int? divisionId)
        {
            var userId = authenticationService.GetCurrentUserId();
            if (userId <= 0)
            {
                return Unauthorized();
            }

            var institutions = await adminService.GetInstitutionsForUser(userId, stateId, divisionId);
            return new JsonResult(institutions);
        }

        public async Task<IActionResult> OnGetGradeSectionsAsync(int institutionId)
        {
            var userId = authenticationService.GetCurrentUserId();
            if (userId <= 0)
            {
                return new JsonResult(Array.Empty<object>());
            }

            if (institutionId <= 0)
            {
                return new JsonResult(Array.Empty<object>());
            }

            var allowed = await adminService.GetInstitutionsForUser(userId, null, null);
            if (!allowed.Any(x => x.Value == institutionId))
            {
                return new JsonResult(Array.Empty<object>());
            }

            var gradeSections = await studentService.GetGradeSectionsByInstitutionId(institutionId);
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
            ReportRows = (await studentFollowupService.GetFollowupReport(userId, BuildFilter())).ToList();
            HasSearched = true;
        }

        private StudentFollowupReportFilterDTO BuildFilter() => new()
        {
            StateId = StateId > 0 ? StateId : null,
            DivisionId = DivisionId > 0 ? DivisionId : null,
            InstitutionId = InstitutionId > 0 ? InstitutionId : null,
            GradeId = GradeId > 0 ? GradeId : null,
            Section = string.IsNullOrWhiteSpace(Section) ? null : Section.Trim(),
            FromDate = FromDate.Date,
            ToDate = ToDate.Date
        };

        private async Task<bool> ValidateFiltersAsync(int userId)
        {
            await LoadFiltersAsync(userId);

            if (FromDate == default || ToDate == default)
            {
                ModelState.AddModelError(string.Empty, "Visit From and Visit To dates are required.");
            }
            else if (FromDate > ToDate)
            {
                ModelState.AddModelError(string.Empty, "Visit From cannot be later than Visit To.");
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
            States = await adminService.GetStatesForUser(userId);
            if (StateId is > 0 && !States.Any(s => s.Value == StateId.Value))
            {
                StateId = null;
            }

            Divisions = await adminService.GetDivisionsForUser(userId, StateId > 0 ? StateId : null);
            if (DivisionId is > 0 && !Divisions.Any(d => d.Value == DivisionId.Value))
            {
                DivisionId = null;
            }

            Institutions = await adminService.GetInstitutionsForUser(userId, StateId, DivisionId);
            if (InstitutionId is > 0 && !Institutions.Any(i => i.Value == InstitutionId.Value))
            {
                InstitutionId = null;
                GradeId = null;
                Section = null;
            }

            Grades = [];
            SectionOptions = [];
            if (InstitutionId is > 0)
            {
                var gradeSections = await studentService.GetGradeSectionsByInstitutionId(InstitutionId.Value);
                Grades = gradeSections
                    .Select(g => new DropdownDTO { Value = g.Id, Text = g.GradeName })
                    .OrderBy(x => x.Text);

                if (GradeId is > 0)
                {
                    var selected = gradeSections.FirstOrDefault(g => g.Id == GradeId);
                    SectionOptions = SplitSections(selected?.Sections);
                    if (!string.IsNullOrWhiteSpace(Section) && !SectionOptions.Contains(Section, StringComparer.OrdinalIgnoreCase))
                    {
                        Section = null;
                    }
                }
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

        private static string FormatPercent(float? value) =>
            value.HasValue ? value.Value.ToString("0.##") : string.Empty;

        private static List<(string Header, Func<StudentFollowupListDTO, string?> Getter)> GetExcelColumns() =>
        [
            ("Visit Date", x => x.FollowupDate.ToString("dd-MMM-yyyy")),
            ("Institution", x => x.InstitutionName),
            ("Grade", x => x.GradeName),
            ("Section", x => x.Section),
            ("Incharge", x => x.InchargeName),
            ("Contact", x => x.InchargeContactNumber),
            ("Sit together", x => x.IsChildSitTogether),
            ("Last month present", x => x.LastMonthAttendanceCount?.ToString()),
            ("Last month working days", x => x.LastMonthWorkingDayCount?.ToString()),
            ("Last month %", x => FormatPercent(x.LastMonthAttendancePercentage)),
            ("Male", x => x.MaleStudentCount?.ToString()),
            ("Female", x => x.FemaleStudentCount?.ToString()),
            ("Present today", x => x.TodayStudentPresentCount?.ToString()),
            ("Total students", x => x.TotalStudentCount?.ToString()),
            ("Today %", x => FormatPercent(x.TotalStudentPercentage)),
            ("Created By", x => x.CreatedByName)
        ];
    }
}
