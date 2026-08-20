using Core.Features.StudentsWeb;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.StudentPortal.Pages;

public sealed class MainstreamModel(
    StudentsWebService studentsService,
    AuthenticationService authenticationService)
    : StudentPortalPageModel(studentsService, authenticationService)
{
    [BindProperty]
    public StudentsWebMainstreamDTO Mainstream { get; set; } = new();

    public IReadOnlyList<StudentsWebLookupDTO> States { get; private set; } = [];
    public IReadOnlyList<StudentsWebLookupDTO> Districts { get; private set; } = [];
    public IReadOnlyList<StudentsWebLookupDTO> Institutions { get; private set; } = [];
    public IReadOnlyList<StudentsWebLookupDTO> GradeSections { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(int studentId)
    {
        var denied = await RequirePageAsync("/StudentPortal/MyInstitution");
        if (denied is not null) return denied;

        var model = await StudentsService.GetMainstream(studentId, CurrentUserId);
        if (model is null) return NotFound();
        if (!model.IsEligible)
        {
            TempData["ErrorMessage"] = model.HasExistingMainstream
                ? "This student has already been mainstreamed."
                : "Completed baseline and endline assessments are required before mainstreaming.";
            return Redirect("/StudentPortal/MyInstitution");
        }

        Mainstream = model;
        Mainstream.IsMainstreamInstitutionSame = true;
        Mainstream.MainstreamInstitutionId = model.EnrolledInstitutionId;
        Mainstream.MainstreamDate = DateTime.Today;
        await LoadLookups();
        return Page();
    }

    public async Task<IActionResult> OnGetDistrictsAsync(int stateId)
    {
        var denied = await RequirePageAsync("/StudentPortal/MyInstitution");
        if (denied is not null) return new JsonResult(Array.Empty<object>()) { StatusCode = 403 };
        return new JsonResult(await StudentsService.GetDistricts(stateId));
    }

    public async Task<IActionResult> OnGetInstitutionsAsync(int stateId, int districtId)
    {
        var denied = await RequirePageAsync("/StudentPortal/MyInstitution");
        if (denied is not null) return new JsonResult(Array.Empty<object>()) { StatusCode = 403 };
        return new JsonResult(await StudentsService.GetMainstreamInstitutions(CurrentUserId, stateId, districtId));
    }

    public async Task<IActionResult> OnGetGradeSectionsAsync(int institutionId)
    {
        var denied = await RequirePageAsync("/StudentPortal/MyInstitution");
        if (denied is not null) return new JsonResult(Array.Empty<object>()) { StatusCode = 403 };
        return new JsonResult(await StudentsService.GetMainstreamGrades());
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var denied = await RequirePageAsync("/StudentPortal/MyInstitution");
        if (denied is not null) return denied;

        var result = await StudentsService.SaveMainstream(Mainstream, CurrentUserId);
        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return Redirect("/StudentPortal/MyInstitution");
        }

        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
        if (result.Errors.Count == 0) ModelState.AddModelError(string.Empty, result.Message);

        var current = await StudentsService.GetMainstream(Mainstream.StudentId, CurrentUserId);
        if (current is null) return NotFound();
        Mainstream.StudentName = current.StudentName;
        Mainstream.StudentCode = current.StudentCode;
        Mainstream.EnrolledInstitutionId = current.EnrolledInstitutionId;
        Mainstream.EnrolledInstitutionName = current.EnrolledInstitutionName;
        Mainstream.EnrolledGradeName = current.EnrolledGradeName;
        Mainstream.EnrollmentDate = current.EnrollmentDate;
        await LoadLookups();
        return Page();
    }

    private async Task LoadLookups()
    {
        States = await StudentsService.GetStates();
        if (Mainstream.StateId.HasValue)
            Districts = await StudentsService.GetDistricts(Mainstream.StateId.Value);
        if (Mainstream.StateId.HasValue && Mainstream.DistrictId.HasValue)
            Institutions = await StudentsService.GetMainstreamInstitutions(
                CurrentUserId, Mainstream.StateId.Value, Mainstream.DistrictId.Value);

        GradeSections = await StudentsService.GetMainstreamGrades();
    }
}
