using Core.DTOs;
using Core.DTOs.App;
using Core.Features.Admin;
using Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebUI.Services;

namespace WebUI.Pages.Admin
{
    public class StudentAnalyticsModel(
        StudentService studentService,
        AdminService adminService,
        AuthenticationService authenticationService) : PageModel
    {
        public StudentAnalyticsSummaryDTO Summary { get; private set; } = new();
        public IReadOnlyList<StudentAnalyticsFunnelItemDTO> FunnelItems { get; private set; } = [];
        public IReadOnlyList<StudentAnalyticsBreakdownDTO> BreakdownRows { get; private set; } = [];

        public IEnumerable<DropdownDTO> States { get; set; } = [];
        public IEnumerable<DropdownDTO> Divisions { get; set; } = [];

        [BindProperty(SupportsGet = true)]
        public int? StateId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? DivisionId { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool IncludeAll { get; set; } = true;

        [BindProperty(SupportsGet = true)]
        public bool IncludeKadam { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool IncludeKadamPlus { get; set; }

        [BindProperty(SupportsGet = true)]
        public string GroupBy { get; set; } = "DIVISION";

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = authenticationService.GetCurrentUserId();
            if (userId <= 0)
                return RedirectToPage("/Login");

            NormalizeGroupBy();
            await LoadDropdownsAsync(userId);
            ClampDivisionId();

            if (FromDate.HasValue && ToDate.HasValue && FromDate > ToDate)
            {
                ModelState.AddModelError(string.Empty, "From Date cannot be later than To Date.");
                return Page();
            }

            await LoadDataAsync(userId);
            return Page();
        }

        public async Task<IActionResult> OnGetDownloadExcelAsync()
        {
            var userId = authenticationService.GetCurrentUserId();
            if (userId <= 0)
                return RedirectToPage("/Login");

            NormalizeGroupBy();
            await LoadDropdownsAsync(userId);
            ClampDivisionId();

            if (FromDate.HasValue && ToDate.HasValue && FromDate > ToDate)
            {
                ModelState.AddModelError(string.Empty, "From Date cannot be later than To Date.");
                return Page();
            }

            var filter = BuildExceptionFilter();
            var data = await studentService.GetStudentAnalyticsBreakdown(userId, filter);
            var bytes = StudentAnalyticsExcelBuilder.BuildBreakdown(data, GroupBy);
            return File(bytes, StudentAnalyticsExcelBuilder.ContentType, StudentAnalyticsExcelBuilder.CreateBreakdownFileName());
        }

        public async Task<IActionResult> OnGetDivisionsByState(int? stateId)
        {
            var userId = authenticationService.GetCurrentUserId();
            if (userId <= 0)
                return Unauthorized();

            var divisions = await adminService.GetDivisionsForUser(userId, stateId > 0 ? stateId : null);
            return new JsonResult(divisions);
        }

        private async Task LoadDataAsync(int userId)
        {
            var filter = BuildFilter();
            Summary = await studentService.GetStudentAnalyticsSummary(userId, filter);
            FunnelItems = StudentService.BuildStudentAnalyticsFunnel(Summary);
            BreakdownRows = (await studentService.GetStudentAnalyticsBreakdown(userId, BuildExceptionFilter())).ToList();
        }

        private KadamProgrammeReportFilterDTO BuildFilter()
        {
            var includeAll = IncludeAll || (!IncludeKadam && !IncludeKadamPlus);
            return new KadamProgrammeReportFilterDTO
            {
                StateId = StateId > 0 ? StateId : null,
                DivisionId = DivisionId > 0 ? DivisionId : null,
                FromDate = FromDate,
                ToDate = ToDate,
                IncludeAll = includeAll,
                IncludeKadam = includeAll ? false : IncludeKadam,
                IncludeKadamPlus = includeAll ? false : IncludeKadamPlus
            };
        }

        private StudentExceptionFilterDTO BuildExceptionFilter()
        {
            var baseFilter = BuildFilter();
            return new StudentExceptionFilterDTO
            {
                StateId = baseFilter.StateId,
                DivisionId = baseFilter.DivisionId,
                FromDate = baseFilter.FromDate,
                ToDate = baseFilter.ToDate,
                IncludeAll = baseFilter.IncludeAll,
                IncludeKadam = baseFilter.IncludeKadam,
                IncludeKadamPlus = baseFilter.IncludeKadamPlus,
                GroupBy = GroupBy
            };
        }

        private void NormalizeGroupBy()
        {
            GroupBy = string.Equals(GroupBy, "INSTITUTION", StringComparison.OrdinalIgnoreCase)
                ? "INSTITUTION"
                : "DIVISION";
        }

        private async Task LoadDropdownsAsync(int userId)
        {
            States = await adminService.GetStatesForUser(userId);
            ClampStateId();
            Divisions = await adminService.GetDivisionsForUser(userId, StateId > 0 ? StateId : null);
        }

        private void ClampStateId()
        {
            if (!StateId.HasValue || StateId.Value <= 0)
            {
                StateId = null;
                return;
            }

            if (!States.Select(s => s.Value).Contains(StateId.Value))
                StateId = null;
        }

        private void ClampDivisionId()
        {
            if (!DivisionId.HasValue || DivisionId.Value <= 0)
            {
                DivisionId = null;
                return;
            }

            if (!Divisions.Select(d => d.Value).Contains(DivisionId.Value))
                DivisionId = null;
        }
    }
}
