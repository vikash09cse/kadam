using Core.DTOs;
using Core.DTOs.App;
using Core.Features.Admin;
using Core.Features.StudentsWeb;
using Microsoft.AspNetCore.Mvc;
using WebUI.Services;

namespace WebUI.Areas.StudentPortal.Pages;

public sealed class ThemeActivityReportModel(
    StudentsWebService studentsService,
    AuthenticationService authenticationService,
    ThemeActivityService themeActivityService,
    StudentService studentService,
    AdminService adminService)
    : StudentPortalPageModel(studentsService, authenticationService)
{
    private const string PageUrl = "/StudentPortal/ThemeActivityReport";

    [BindProperty]
    public int? StateId { get; set; }

    [BindProperty]
    public int? DivisionId { get; set; }

    [BindProperty]
    public int? InstitutionId { get; set; }

    [BindProperty]
    public int? ThemeId { get; set; }

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
    public IEnumerable<DropdownDTO> Themes { get; private set; } = [];
    public IEnumerable<DropdownDTO> Grades { get; private set; } = [];
    public IEnumerable<string> SectionOptions { get; private set; } = [];
    public IReadOnlyList<ThemeActivityReportDTO> ReportRows { get; private set; } = [];
    public bool HasSearched { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var denied = await RequirePageAsync(PageUrl);
        if (denied is not null) return denied;

        SetDefaultDateRange();
        await LoadFiltersAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostSearchAsync()
    {
        var denied = await RequirePageAsync(PageUrl);
        if (denied is not null) return denied;

        if (!await ValidateFiltersAsync())
        {
            await LoadFiltersAsync();
            return Page();
        }

        await LoadReportAsync();
        await LoadFiltersAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDownloadExcelAsync()
    {
        var denied = await RequirePageAsync(PageUrl);
        if (denied is not null) return denied;

        if (!await ValidateFiltersAsync())
        {
            await LoadFiltersAsync();
            return Page();
        }

        try
        {
            var data = await themeActivityService.GetThemeActivityReport(CurrentUserId, BuildFilter());
            var bytes = ThemeActivityReportExcelBuilder.Build(data);
            return File(bytes, ThemeActivityReportExcelBuilder.ContentType, ThemeActivityReportExcelBuilder.CreateFileName());
        }
        catch (Exception ex)
        {
            await LoadFiltersAsync();
            ModelState.AddModelError(string.Empty, $"Unable to generate Excel. {ex.Message}");
            return Page();
        }
    }

    public async Task<IActionResult> OnGetDivisionsByState(int? stateId)
    {
        var denied = await RequirePageAsync(PageUrl);
        if (denied is not null) return Unauthorized();

        var divisions = await adminService.GetDivisionsForUser(CurrentUserId, stateId > 0 ? stateId : null);
        return new JsonResult(divisions);
    }

    public async Task<IActionResult> OnGetInstitutions(int? stateId, int? divisionId)
    {
        var denied = await RequirePageAsync(PageUrl);
        if (denied is not null) return Unauthorized();

        var institutions = await adminService.GetInstitutionsForUser(CurrentUserId, stateId, divisionId);
        return new JsonResult(institutions);
    }

    public async Task<IActionResult> OnGetGradeSectionsAsync(int institutionId)
    {
        var denied = await RequirePageAsync(PageUrl);
        if (denied is not null) return new JsonResult(Array.Empty<object>());

        if (institutionId <= 0)
            return new JsonResult(Array.Empty<object>());

        var allowed = await adminService.GetInstitutionsForUser(CurrentUserId, null, null);
        if (!allowed.Any(x => x.Value == institutionId))
            return new JsonResult(Array.Empty<object>());

        var gradeSections = await studentService.GetGradeSectionsByInstitutionId(institutionId);
        return new JsonResult(gradeSections.Select(g => new
        {
            id = g.Id,
            gradeName = g.GradeName,
            sections = SplitSections(g.Sections)
        }));
    }

    private async Task LoadReportAsync()
    {
        ReportRows = (await themeActivityService.GetThemeActivityReport(CurrentUserId, BuildFilter())).ToList();
        HasSearched = true;
    }

    private ThemeActivityReportFilterDTO BuildFilter() => new()
    {
        StateId = StateId > 0 ? StateId : null,
        DivisionId = DivisionId > 0 ? DivisionId : null,
        InstitutionId = InstitutionId > 0 ? InstitutionId : null,
        ThemeId = ThemeId > 0 ? ThemeId : null,
        GradeId = GradeId > 0 ? GradeId : null,
        Section = string.IsNullOrWhiteSpace(Section) ? null : Section.Trim(),
        FromDate = FromDate.Date,
        ToDate = ToDate.Date
    };

    private async Task<bool> ValidateFiltersAsync()
    {
        await LoadFiltersAsync();

        if (FromDate == default || ToDate == default)
            ModelState.AddModelError(string.Empty, "Activity From and Activity To dates are required.");
        else if (FromDate > ToDate)
            ModelState.AddModelError(string.Empty, "Activity From cannot be later than Activity To.");

        return ModelState.IsValid;
    }

    private void SetDefaultDateRange()
    {
        var today = DateTime.Today;
        FromDate = new DateTime(today.Year, today.Month, 1);
        ToDate = FromDate.AddMonths(1).AddDays(-1);
    }

    private async Task LoadFiltersAsync()
    {
        States = await adminService.GetStatesForUser(CurrentUserId);
        if (StateId is > 0 && !States.Any(s => s.Value == StateId.Value))
            StateId = null;

        Divisions = await adminService.GetDivisionsForUser(CurrentUserId, StateId > 0 ? StateId : null);
        if (DivisionId is > 0 && !Divisions.Any(d => d.Value == DivisionId.Value))
            DivisionId = null;

        Institutions = await adminService.GetInstitutionsForUser(CurrentUserId, StateId, DivisionId);
        if (InstitutionId is > 0 && !Institutions.Any(i => i.Value == InstitutionId.Value))
        {
            InstitutionId = null;
            GradeId = null;
            Section = null;
        }

        Themes = await adminService.GetActiveThemes();
        if (ThemeId is > 0 && !Themes.Any(t => t.Value == ThemeId.Value))
            ThemeId = null;

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
                    Section = null;
            }
        }
    }

    private static List<string> SplitSections(string? sections)
    {
        if (string.IsNullOrWhiteSpace(sections)) return [];
        return sections
            .Split([',', ';', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(s => s)
            .ToList();
    }
}
