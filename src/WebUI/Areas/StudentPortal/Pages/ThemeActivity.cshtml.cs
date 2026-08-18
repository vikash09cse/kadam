using Core.Features.StudentsWeb;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace WebUI.Areas.StudentPortal.Pages;

[ValidateAntiForgeryToken]
public sealed class ThemeActivityModel(
    StudentsWebService studentsService,
    AuthenticationService authenticationService)
    : StudentPortalPageModel(studentsService, authenticationService)
{
    private const string PageUrl = "/StudentPortal/ThemeActivities";

    [BindProperty]
    public StudentsWebThemeActivitySaveDTO Input { get; set; } = new();
    public StudentsWebThemeActivityDTO? Detail { get; private set; }
    public IReadOnlyList<StudentsWebLookupDTO> Institutions { get; private set; } = [];
    public IReadOnlyList<StudentsWebLookupDTO> Themes { get; private set; } = [];
    public bool IsView { get; private set; }
    public DateTime Today => DateTime.Today;
    public DateTime MinimumDate =>
        Input.Id > 0 && Input.ActivityDate.Date < DateTime.Today.AddMonths(-1)
            ? Input.ActivityDate.Date
            : DateTime.Today.AddMonths(-1);

    public async Task<IActionResult> OnGetAsync(int? id, string? mode)
    {
        var denied = await RequirePageAsync(PageUrl);
        if (denied is not null) return denied;

        await LoadSelectionsAsync();
        IsView = string.Equals(mode, "view", StringComparison.OrdinalIgnoreCase);
        if (!id.HasValue)
        {
            Input.ActivityDate = DateTime.Today;
            return Page();
        }

        Detail = await StudentsService.GetThemeActivity(id.Value, CurrentUserId);
        if (Detail is null) return NotFound();
        Input = new StudentsWebThemeActivitySaveDTO
        {
            Id = Detail.Id,
            ActivityDate = Detail.ActivityDate,
            InstitutionId = Detail.InstitutionId,
            ThemeId = Detail.ThemeId,
            GradeSections = Detail.GradeSections,
            StudentsAttended = Detail.StudentsAttended,
            DidChildrensDayHappen = Detail.DidChildrensDayHappen,
            ParentsAttended = Detail.ParentsAttended
        };
        if (!Institutions.Any(x => x.Id == Detail.InstitutionId))
            Institutions = Institutions.Append(new StudentsWebLookupDTO
            {
                Id = Detail.InstitutionId,
                Text = Detail.InstitutionName
            }).ToList();
        if (!Themes.Any(x => x.Id == Detail.ThemeId))
            Themes = Themes.Append(new StudentsWebLookupDTO
            {
                Id = Detail.ThemeId,
                Text = Detail.ThemeName
            }).ToList();
        return Page();
    }

    public async Task<IActionResult> OnGetGradeSectionsAsync(int institutionId)
    {
        var denied = await RequirePageAsync(PageUrl);
        if (denied is not null)
            return new JsonResult(Array.Empty<object>()) { StatusCode = 403 };
        return new JsonResult(
            await StudentsService.GetThemeActivityGradeSections(CurrentUserId, institutionId));
    }

    public async Task<IActionResult> OnGetEligibleCountAsync(int institutionId, string? gradeSections)
    {
        var denied = await RequirePageAsync(PageUrl);
        if (denied is not null)
            return new JsonResult(new { total = 0 }) { StatusCode = 403 };

        try
        {
            var selections = JsonSerializer.Deserialize<List<StudentsWebThemeActivityGradeSectionDTO>>(
                gradeSections ?? "[]",
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
            var total = await StudentsService.GetThemeActivityEligibleCount(
                CurrentUserId, institutionId, selections);
            return new JsonResult(new { total });
        }
        catch (JsonException)
        {
            return new JsonResult(new { total = 0 }) { StatusCode = 400 };
        }
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        var denied = await RequirePageAsync(PageUrl);
        if (denied is not null) return denied;

        var result = await StudentsService.SaveThemeActivity(Input, CurrentUserId);
        if (!result.Success)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error);
            if (result.Errors.Count == 0)
                ModelState.AddModelError(string.Empty, result.Message);
            await LoadSelectionsAsync();
            return Page();
        }

        TempData["SuccessMessage"] = result.Message;
        return Redirect("/StudentPortal/ThemeActivities");
    }

    private async Task LoadSelectionsAsync()
    {
        Institutions = await StudentsService.GetInstitutions(CurrentUserId);
        Themes = await StudentsService.GetActiveThemes();
    }
}
