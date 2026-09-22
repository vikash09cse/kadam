using Core.DTOs;
using Core.Features.Admin;
using Core.Features.StudentsWeb;
using Microsoft.AspNetCore.Mvc;
using WebUI.Services;

namespace WebUI.Areas.StudentPortal.Pages;

public sealed class ReportModel(
    StudentsWebService studentsService,
    AuthenticationService authenticationService,
    StudentService studentService,
    AdminService adminService)
    : StudentPortalPageModel(studentsService, authenticationService)
{
    private const string PageUrl = "/StudentPortal/Report";

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
        var denied = await RequirePageAsync(PageUrl);
        if (denied is not null) return denied;

        await LoadDropdownsAsync();
        return Page();
    }

    public async Task<IActionResult> OnGetDivisionsByState(int? stateId)
    {
        var denied = await RequirePageAsync(PageUrl);
        if (denied is not null) return Unauthorized();

        var divisions = await adminService.GetDivisionsForUser(CurrentUserId, stateId > 0 ? stateId : null);
        return new JsonResult(divisions);
    }

    public async Task<IActionResult> OnPostDownloadReportAsync()
    {
        var denied = await RequirePageAsync(PageUrl);
        if (denied is not null) return denied;

        await LoadDropdownsAsync();
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

            var data = await studentService.GetKadamProgrammeReport(CurrentUserId, filter);
            var bytes = KadamProgrammeReportExcelBuilder.Build(data);
            return File(bytes, KadamProgrammeReportExcelBuilder.ContentType, KadamProgrammeReportExcelBuilder.CreateFileName());
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Unable to generate report. {ex.Message}");
            return Page();
        }
    }

    private async Task LoadDropdownsAsync()
    {
        States = await adminService.GetStatesForUser(CurrentUserId);
        ClampStateId();
        Divisions = await adminService.GetDivisionsForUser(CurrentUserId, StateId > 0 ? StateId : null);
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
            StateId = null;
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
