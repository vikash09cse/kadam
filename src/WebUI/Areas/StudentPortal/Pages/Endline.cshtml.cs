using Core.Features.StudentsWeb;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.StudentPortal.Pages;

public sealed class EndlineModel(
    StudentsWebService studentsService,
    AuthenticationService authenticationService)
    : StudentPortalPageModel(studentsService, authenticationService)
{
    [BindProperty]
    public StudentsWebAssessmentSaveDTO Assessment { get; set; } = new();

    public StudentsWebAssessmentDTO Details { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(int studentId)
    {
        var denied = await RequirePageAsync("/StudentPortal/MyInstitution");
        if (denied is not null) return denied;

        var details = await StudentsService.GetAssessment(
            studentId, StudentsWebAssessmentKind.Endline, CurrentUserId);
        if (details is null) return NotFound();
        Details = details;
        Assessment = new StudentsWebAssessmentSaveDTO
        {
            StudentId = studentId,
            CompletedDate = details.CompletedDate ?? DateTime.Today,
            Scores = details.Subjects.Select(x => new StudentsWebAssessmentScoreInputDTO
            {
                SubjectId = x.SubjectId,
                ObtainedMarks = x.ObtainedMarks
            }).ToList()
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var denied = await RequirePageAsync("/StudentPortal/MyInstitution");
        if (denied is not null) return denied;

        var result = await StudentsService.SaveEndline(Assessment, CurrentUserId);
        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return Redirect("/StudentPortal/MyInstitution");
        }

        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
        if (result.Errors.Count == 0) ModelState.AddModelError(string.Empty, result.Message);
        var details = await StudentsService.GetAssessment(
            Assessment.StudentId, StudentsWebAssessmentKind.Endline, CurrentUserId);
        if (details is null) return NotFound();
        Details = details;
        return Page();
    }
}
