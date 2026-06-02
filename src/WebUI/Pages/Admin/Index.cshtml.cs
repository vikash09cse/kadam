using Core.DTOs;
using Core.DTOs.App;
using Core.Features.Admin;
using Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages.Admin
{
    public class IndexModel(StudentService studentService, AdminService adminService, AuthenticationService authenticationService) : PageModel
    {
        public int ActiveCount { get; private set; }
        public int InactiveCount { get; private set; }
        public int CompletedCount { get; private set; }

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

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = authenticationService.GetCurrentUserId();
            if (userId <= 0)
            {
                return RedirectToPage("/Login");
            }

            await LoadDropdownsAsync();

            if (FromDate.HasValue && ToDate.HasValue && FromDate > ToDate)
            {
                ModelState.AddModelError(string.Empty, "From Date cannot be later than To Date.");
                return Page();
            }

            var response = await studentService.GetAdminDashboardCount(userId, BuildFilter());
            if (response.Result is DashboardDTO counts)
            {
                ActiveCount = counts.ActiveCount;
                InactiveCount = counts.InactiveCount;
                CompletedCount = counts.CompletedCount;
            }

            return Page();
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

        private async Task LoadDropdownsAsync()
        {
            States = await adminService.GetStatesByStatus(Enums.Status.Active);
            Divisions = await adminService.GetDivisionsByStatus(Enums.Status.Active);
        }
    }
}
