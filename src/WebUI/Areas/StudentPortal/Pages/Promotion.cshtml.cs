using Core.Features.StudentsWeb;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.StudentPortal.Pages;

public sealed class PromotionModel(
    StudentsWebService studentsService,
    AuthenticationService authenticationService)
    : StudentPortalPageModel(studentsService, authenticationService)
{
    [BindProperty]
    public StudentsWebPromotionDTO Promotion { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int studentId)
    {
        var denied = await RequirePageAsync("/StudentPortal/MyInstitution");
        if (denied is not null) return denied;
        var model = await StudentsService.GetPromotion(studentId, CurrentUserId);
        if (model is null) return NotFound();
        if (!model.IsEligible)
        {
            TempData["ErrorMessage"] = "Only eligible Kadam Plus students can be promoted.";
            return Redirect("/StudentPortal/MyInstitution");
        }
        Promotion = model;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var denied = await RequirePageAsync("/StudentPortal/MyInstitution");
        if (denied is not null) return denied;
        var result = await StudentsService.PromoteStudent(Promotion, CurrentUserId);
        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return Redirect("/StudentPortal/MyInstitution");
        }

        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
        if (result.Errors.Count == 0) ModelState.AddModelError(string.Empty, result.Message);
        var current = await StudentsService.GetPromotion(Promotion.StudentId, CurrentUserId);
        if (current is null) return NotFound();
        current.DestinationGradeId = Promotion.DestinationGradeId;
        current.DestinationSection = Promotion.DestinationSection;
        current.PromotionDate = Promotion.PromotionDate;
        Promotion = current;
        return Page();
    }
}
