using Core.Features.StudentsWeb;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.StudentPortal.Pages;

public sealed class GradeTestModel(
    StudentsWebService studentsService,
    AuthenticationService authenticationService)
    : StudentPortalPageModel(studentsService, authenticationService)
{
    [BindProperty]
    public StudentsWebGradeTestSaveDTO GradeTest { get; set; } = new();
    public StudentsWebGradeTestDTO Details { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(int studentId, int gradeLevelId)
    {
        var denied = await RequirePageAsync("/StudentPortal/MyInstitution");
        if (denied is not null) return denied;
        var details = await StudentsService.GetGradeTest(studentId, gradeLevelId, CurrentUserId);
        if (details is null) return NotFound();
        Details = details;
        GradeTest = new StudentsWebGradeTestSaveDTO
        {
            StudentId = studentId,
            GradeLevelId = gradeLevelId,
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
        var result = await StudentsService.SaveGradeTest(GradeTest, CurrentUserId);
        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return Redirect($"/StudentPortal/Progress?studentId={GradeTest.StudentId}");
        }
        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
        if (result.Errors.Count == 0) ModelState.AddModelError(string.Empty, result.Message);
        var details = await StudentsService.GetGradeTest(GradeTest.StudentId, GradeTest.GradeLevelId, CurrentUserId);
        if (details is null) return NotFound();
        Details = details;
        return Page();
    }
}
