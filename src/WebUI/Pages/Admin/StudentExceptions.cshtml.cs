using Core.DTOs;
using Core.DTOs.App;
using Core.Features.Admin;
using Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebUI.Services;

namespace WebUI.Pages.Admin
{
    public class StudentExceptionsModel(
        StudentService studentService,
        AdminService adminService,
        AuthenticationService authenticationService) : PageModel
    {
        public static readonly IReadOnlyList<(string Code, string Label)> ExceptionOptions =
        [
            ("NO_BASELINE", "No baseline"),
            ("STUCK_AFTER_BASELINE", "Stuck after baseline"),
            ("MISSING_GRADE_TEST", "Missing grade test"),
            ("READY_FOR_ENDLINE", "Ready for endline"),
            ("READY_FOR_MAINSTREAM", "Ready for mainstream"),
            ("LONG_INACTIVE", "Long inactive")
        ];

        public IReadOnlyList<StudentExceptionListItemDTO> Rows { get; private set; } = [];
        public int TotalCount { get; private set; }
        public int TotalPages { get; private set; }

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
        public string? ExceptionCode { get; set; }

        [BindProperty(SupportsGet = true)]
        public int DaysNoBaseline { get; set; } = 30;

        [BindProperty(SupportsGet = true)]
        public int DaysStuck { get; set; } = 14;

        [BindProperty(SupportsGet = true)]
        public int DaysInactive { get; set; } = 30;

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 50;

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = authenticationService.GetCurrentUserId();
            if (userId <= 0)
                return RedirectToPage("/Login");

            NormalizePaging();
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

            NormalizePaging();
            await LoadDropdownsAsync(userId);
            ClampDivisionId();

            if (FromDate.HasValue && ToDate.HasValue && FromDate > ToDate)
            {
                ModelState.AddModelError(string.Empty, "From Date cannot be later than To Date.");
                return Page();
            }

            var filter = BuildFilter();
            filter.Page = 1;
            filter.PageSize = 100000;
            var data = await studentService.GetStudentExceptions(userId, filter);
            var bytes = StudentAnalyticsExcelBuilder.BuildExceptions(data);
            return File(bytes, StudentAnalyticsExcelBuilder.ContentType, StudentAnalyticsExcelBuilder.CreateExceptionsFileName());
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
            Rows = (await studentService.GetStudentExceptions(userId, BuildFilter())).ToList();
            TotalCount = Rows.FirstOrDefault()?.TotalCount ?? 0;
            TotalPages = TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
        }

        private StudentExceptionFilterDTO BuildFilter()
        {
            var includeAll = IncludeAll || (!IncludeKadam && !IncludeKadamPlus);
            return new StudentExceptionFilterDTO
            {
                StateId = StateId > 0 ? StateId : null,
                DivisionId = DivisionId > 0 ? DivisionId : null,
                FromDate = FromDate,
                ToDate = ToDate,
                IncludeAll = includeAll,
                IncludeKadam = includeAll ? false : IncludeKadam,
                IncludeKadamPlus = includeAll ? false : IncludeKadamPlus,
                ExceptionCode = string.IsNullOrWhiteSpace(ExceptionCode) ? null : ExceptionCode.Trim(),
                DaysNoBaseline = DaysNoBaseline > 0 ? DaysNoBaseline : 30,
                DaysStuck = DaysStuck > 0 ? DaysStuck : 14,
                DaysInactive = DaysInactive > 0 ? DaysInactive : 30,
                Page = PageNumber,
                PageSize = PageSize
            };
        }

        private void NormalizePaging()
        {
            if (PageNumber < 1) PageNumber = 1;
            if (PageSize < 1) PageSize = 50;
            if (PageSize > 200) PageSize = 200;
            if (DaysNoBaseline < 0) DaysNoBaseline = 30;
            if (DaysStuck < 0) DaysStuck = 14;
            if (DaysInactive < 0) DaysInactive = 30;

            if (!string.IsNullOrWhiteSpace(ExceptionCode)
                && !ExceptionOptions.Any(x => x.Code.Equals(ExceptionCode, StringComparison.OrdinalIgnoreCase)))
            {
                ExceptionCode = null;
            }
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
