using Core.Features.StudentsWeb;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.StudentPortal.Pages;

[ValidateAntiForgeryToken]
public sealed class FollowupModel(
    StudentsWebService studentsService,
    AuthenticationService authenticationService)
    : StudentPortalPageModel(studentsService, authenticationService)
{
    private const string PageUrl = "/StudentPortal/Followups";

    [BindProperty]
    public StudentsWebFollowupSaveDTO Input { get; set; } = new();
    public StudentsWebFollowupDTO? Detail { get; private set; }
    public IReadOnlyList<StudentsWebLookupDTO> Institutions { get; private set; } = [];
    public bool IsView { get; private set; }
    public DateTime Today => DateTime.Today;

    public async Task<IActionResult> OnGetAsync(int? id, string? mode)
    {
        var denied = await RequirePageAsync(PageUrl);
        if (denied is not null) return denied;

        Institutions = await StudentsService.GetInstitutions(CurrentUserId);
        IsView = string.Equals(mode, "view", StringComparison.OrdinalIgnoreCase);
        if (!id.HasValue)
        {
            Input.VisitDate = DateTime.Today;
            return Page();
        }

        Detail = await StudentsService.GetFollowup(id.Value, CurrentUserId);
        if (Detail is null) return NotFound();
        Input = new StudentsWebFollowupSaveDTO
        {
            Id = Detail.Id,
            VisitDate = Detail.VisitDate,
            InstitutionId = Detail.InstitutionId,
            GradeId = Detail.GradeId,
            Section = Detail.Section,
            TeacherName = Detail.TeacherName,
            TeacherContact = Detail.TeacherContact,
            MaleStudentCount = Detail.MaleStudentCount,
            FemaleStudentCount = Detail.FemaleStudentCount,
            PresentTodayCount = Detail.PresentTodayCount,
            LastMonthWorkingDays = Detail.LastMonthWorkingDays,
            LastMonthAttendance = Detail.LastMonthAttendance,
            ChildrenSitTogether = Detail.ChildrenSitTogether
        };
        return Page();
    }

    public async Task<IActionResult> OnGetGradeSectionsAsync(int institutionId)
    {
        var denied = await RequirePageAsync(PageUrl);
        if (denied is not null)
            return new JsonResult(Array.Empty<object>()) { StatusCode = 403 };

        var institutions = await StudentsService.GetInstitutions(CurrentUserId);
        if (!institutions.Any(x => x.Id == institutionId))
            return new JsonResult(Array.Empty<object>()) { StatusCode = 403 };
        return new JsonResult(await StudentsService.GetGradeSections(institutionId));
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        var denied = await RequirePageAsync(PageUrl);
        if (denied is not null) return denied;

        var result = await StudentsService.SaveFollowup(Input, CurrentUserId);
        if (!result.Success)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error);
            if (result.Errors.Count == 0)
                ModelState.AddModelError(string.Empty, result.Message);
            Institutions = await StudentsService.GetInstitutions(CurrentUserId);
            return Page();
        }

        TempData["SuccessMessage"] = result.Message;
        return Redirect("/StudentPortal/Followups");
    }
}
