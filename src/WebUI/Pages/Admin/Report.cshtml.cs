using Core.DTOs;
using Core.Features.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebUI.Services;

namespace WebUI.Pages.Admin
{
    public class ReportModel(StudentService studentService, AdminService adminService, AuthenticationService authenticationService) : PageModel
    {
        public IEnumerable<DropdownDTO> States { get; set; } = [];
        public IEnumerable<DropdownDTO> Divisions { get; set; } = [];

        [BindProperty]
        public int? StateId { get; set; }

        [BindProperty]
        public bool IncludeAllDivisions { get; set; } = true;

        [BindProperty]
        public List<int> SelectedDivisionIds { get; set; } = [];

        [BindProperty]
        public DateTime? FromDate { get; set; }

        [BindProperty]
        public DateTime? ToDate { get; set; }

        [BindProperty]
        public bool IncludeAll { get; set; } = true;

        [BindProperty]
        public bool IncludeKadam { get; set; }

        [BindProperty]
        public bool IncludeKadamPlus { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = authenticationService.GetCurrentUserId();
            if (userId <= 0)
            {
                return RedirectToPage("/Login");
            }

            await LoadDropdownsAsync(userId);
            return Page();
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

        public async Task<IActionResult> OnPostDownloadReportAsync()
        {
            var userId = authenticationService.GetCurrentUserId();
            if (userId <= 0)
            {
                return RedirectToPage("/Login");
            }

            await LoadDropdownsAsync(userId);
            ClampSelectedDivisionIds();

            if (FromDate.HasValue && ToDate.HasValue && FromDate > ToDate)
            {
                ModelState.AddModelError(string.Empty, "From Date cannot be later than To Date.");
                return Page();
            }

            var includeAll = IncludeAll || (!IncludeKadam && !IncludeKadamPlus);
            var includeAllDivisions = IncludeAllDivisions || SelectedDivisionIds.Count == 0;

            try
            {
                var filter = new KadamProgrammeReportFilterDTO
                {
                    StateId = StateId > 0 ? StateId : null,
                    DivisionIds = includeAllDivisions ? null : SelectedDivisionIds,
                    FromDate = FromDate,
                    ToDate = ToDate,
                    IncludeAll = includeAll,
                    IncludeKadam = includeAll ? false : IncludeKadam,
                    IncludeKadamPlus = includeAll ? false : IncludeKadamPlus
                };

                var data = await studentService.GetKadamProgrammeReport(userId, filter);
                var bytes = KadamProgrammeReportExcelBuilder.Build(data);
                return File(bytes, KadamProgrammeReportExcelBuilder.ContentType, KadamProgrammeReportExcelBuilder.CreateFileName());
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Unable to generate report. {ex.Message}");
                return Page();
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

            var allowedIds = States.Select(s => s.Value).ToHashSet();
            if (!allowedIds.Contains(StateId.Value))
            {
                StateId = null;
            }
        }

        private void ClampSelectedDivisionIds()
        {
            var allowedIds = Divisions.Select(d => d.Value).ToHashSet();
            SelectedDivisionIds = (SelectedDivisionIds ?? [])
                .Where(id => id > 0 && allowedIds.Contains(id))
                .Distinct()
                .ToList();
        }
    }
}
