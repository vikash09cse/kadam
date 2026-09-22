using Core.DTOs;
using Core.DTOs.App;
using Core.Features.Admin;
using Core.Features.StudentsWeb;
using Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebUI.Services;

namespace WebUI.Areas.StudentPortal.Pages;

public sealed class AttendanceReportModel(
    StudentsWebService studentsService,
    AuthenticationService authenticationService,
    StudentService studentService,
    InstitutionService institutionService)
    : StudentPortalPageModel(studentsService, authenticationService)
{
    private const string PageUrl = "/StudentPortal/AttendanceReport";

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
    public string GradeSectionsJson { get; private set; } = "[]";

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
            var data = await studentService.GetStudentAttendanceSummaryReport(CurrentUserId, BuildFilter());
            var bytes = AttendanceSummaryReportExcelBuilder.Build(data);
            return File(bytes, AttendanceSummaryReportExcelBuilder.ContentType, AttendanceSummaryReportExcelBuilder.CreateFileName());
        }
        catch (Exception ex)
        {
            await LoadFiltersAsync();
            ModelState.AddModelError(string.Empty, $"Unable to generate Excel. {ex.Message}");
            return Page();
        }
    }

    public async Task<IActionResult> OnGetGradeSectionsAsync(int institutionId)
    {
        var denied = await RequirePageAsync(PageUrl);
        if (denied is not null) return new JsonResult(Array.Empty<object>());

        IsAdmin = await studentService.IsAdminUser(CurrentUserId);
        IEnumerable<AppGradeSectionDTO> gradeSections;

        if (IsAdmin)
        {
            gradeSections = await studentService.GetGradeSectionsByInstitutionId(institutionId);
        }
        else
        {
            var institutions = await studentService.GetInstitutionsByUserId(CurrentUserId);
            if (!institutions.Any(x => x.Id == institutionId))
                return new JsonResult(Array.Empty<object>());

            gradeSections = institutions.FirstOrDefault(x => x.Id == institutionId)?.GradeSections ?? [];
        }

        return new JsonResult(gradeSections.Select(g => new
        {
            id = g.Id,
            gradeName = g.GradeName,
            sections = SplitSections(g.Sections)
        }));
    }

    private async Task LoadReportAsync()
    {
        ReportRows = (await studentService.GetStudentAttendanceSummaryReport(CurrentUserId, BuildFilter())).ToList();
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

    private async Task<bool> ValidateFiltersAsync()
    {
        if (InstitutionId <= 0)
        {
            ModelState.AddModelError(string.Empty, "Please select an institution.");
        }
        else
        {
            IsAdmin = await studentService.IsAdminUser(CurrentUserId);
            if (!IsAdmin)
            {
                var allowedInstitutions = await studentService.GetInstitutionsByUserId(CurrentUserId);
                if (!allowedInstitutions.Any(x => x.Id == InstitutionId))
                {
                    ModelState.AddModelError(string.Empty, "You do not have access to the selected institution.");
                    InstitutionId = 0;
                }
            }
        }

        if (FromDate == default || ToDate == default)
            ModelState.AddModelError(string.Empty, "From Date and To Date are required.");
        else if (FromDate > ToDate)
            ModelState.AddModelError(string.Empty, "From Date cannot be later than To Date.");

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
        IsAdmin = await studentService.IsAdminUser(CurrentUserId);

        List<AppInstitutionDTO> assignedInstitutions = [];
        if (IsAdmin)
        {
            var institutions = await institutionService.GetInstitutions(1, 10000, Enums.Status.Active, string.Empty);
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
            assignedInstitutions = (await studentService.GetInstitutionsByUserId(CurrentUserId)).ToList();
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

        await BindGradeSectionDropdownsAsync();
    }

    private async Task BindGradeSectionDropdownsAsync()
    {
        Grades = [];
        SectionOptions = [];
        if (InstitutionId <= 0) return;

        IEnumerable<AppGradeSectionDTO> gradeSections;
        if (IsAdmin)
            gradeSections = await studentService.GetGradeSectionsByInstitutionId(InstitutionId);
        else
        {
            var institutions = await studentService.GetInstitutionsByUserId(CurrentUserId);
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
        if (string.IsNullOrWhiteSpace(sections)) return [];
        return sections
            .Split([',', ';', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(s => s)
            .ToList();
    }
}
