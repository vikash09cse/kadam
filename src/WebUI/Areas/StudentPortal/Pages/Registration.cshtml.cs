using Core.Features.StudentsWeb;
using Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebUI.Areas.StudentPortal.Pages;

public sealed class RegistrationModel(
    StudentsWebService studentsService,
    AuthenticationService authenticationService,
    IWebHostEnvironment environment,
    IConfiguration configuration)
    : StudentPortalPageModel(studentsService, authenticationService)
{
    private readonly string profileUploadRoot = ResolveProfileUploadRoot(environment, configuration);
    private readonly string profilePublicBaseUrl = configuration["StudentUploads:PublicBaseUrl"]?.TrimEnd('/') ?? string.Empty;

    [BindProperty]
    public StudentsWebEditDTO Student { get; set; } = new();

    [BindProperty]
    public IFormFile? ProfilePicture { get; set; }

    public IReadOnlyList<StudentsWebLookupDTO> Institutions { get; private set; } = [];
    public IReadOnlyList<StudentsWebLookupDTO> GradeSections { get; private set; } = [];
    public IEnumerable<SelectListItem> Genders => EnumItems<Enums.Gender>();
    public IEnumerable<SelectListItem> ChildStatuses => EnumItems<Enums.ChildStatusBeforKadamType>();
    public IEnumerable<SelectListItem> StayDurations => EnumItems<Enums.HowLongStayInThisAreaType>();
    public IEnumerable<SelectListItem> Reasons => EnumItems<Enums.StudentReasonType>();
    public IEnumerable<SelectListItem> Occupations => EnumItems<Enums.OccupationType>();
    public IEnumerable<SelectListItem> FatherOccupations =>
        EnumItems<Enums.OccupationType>().Where(x => x.Value != ((int)Enums.OccupationType.Housewife).ToString());
    public IEnumerable<SelectListItem> Educations => EnumItems<Enums.EducationType>();
    public IEnumerable<SelectListItem> PeopleCounts => EnumItems<Enums.PeopleLivingCountType>();
    public IEnumerable<SelectListItem> Castes => EnumItems<Enums.CasteType>();
    public IEnumerable<SelectListItem> Religions => EnumItems<Enums.ReligionType>();
    public IEnumerable<SelectListItem> Incomes => EnumItems<Enums.MonthlyIncomeType>();
    public string ProfilePictureUrl => string.IsNullOrWhiteSpace(Student.ProfilePicturePath)
        ? string.Empty
        : $"{profilePublicBaseUrl}{Student.ProfilePicturePath}";

    public async Task<IActionResult> OnGetAsync(int id = 0)
    {
        var denied = await RequirePageAsync("/StudentPortal/Registration");
        if (denied is not null) return denied;

        await LoadLookups();
        if (id > 0)
        {
            var student = await StudentsService.GetStudent(id, CurrentUserId);
            if (student is null) return NotFound();
            Student = student;
            GradeSections = await StudentsService.GetGradeSections(Student.InstitutionId);
        }
        return Page();
    }

    public async Task<IActionResult> OnGetGradeSectionsAsync(int institutionId)
    {
        var denied = await RequirePageAsync("/StudentPortal/Registration");
        if (denied is not null) return new JsonResult(Array.Empty<object>()) { StatusCode = 403 };
        return new JsonResult(await StudentsService.GetGradeSections(institutionId));
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var denied = await RequirePageAsync("/StudentPortal/Registration");
        if (denied is not null) return denied;

        if (Student.Id > 0)
        {
            var existing = await StudentsService.GetStudent(Student.Id, CurrentUserId);
            if (existing is null) return NotFound();
            Student.ProfilePicturePath = existing.ProfilePicturePath;
            Student.IsBaselineAdded = existing.IsBaselineAdded;
            if (existing.IsBaselineAdded)
            {
                Student.EnrollmentDate = existing.EnrollmentDate;
                Student.DateOfBirth = existing.DateOfBirth;
                Student.InstitutionId = existing.InstitutionId;
                Student.GradeId = existing.GradeId;
            }
        }

        string? uploadedPhysicalPath = null;
        if (ProfilePicture is not null)
        {
            var upload = await SaveProfilePicture(ProfilePicture, [".jpg", ".jpeg", ".png", ".webp"], 5 * 1024 * 1024);
            if (!upload.Success)
            {
                ModelState.AddModelError(nameof(ProfilePicture), upload.Error!);
                await LoadLookups();
                return Page();
            }
            Student.ProfilePicturePath = upload.RelativePath;
            uploadedPhysicalPath = upload.PhysicalPath;
        }

        var result = await StudentsService.SaveStudent(Student, CurrentUserId);
        if (!result.Success)
        {
            if (uploadedPhysicalPath is not null && System.IO.File.Exists(uploadedPhysicalPath))
                System.IO.File.Delete(uploadedPhysicalPath);
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            await LoadLookups();
            return Page();
        }

        TempData["SuccessMessage"] = result.Message;
        return Redirect("/StudentPortal/Registration?id=" + result.Id);
    }

    private async Task LoadLookups()
    {
        Institutions = await StudentsService.GetInstitutions(CurrentUserId);
        if (Student.InstitutionId > 0)
            GradeSections = await StudentsService.GetGradeSections(Student.InstitutionId);
    }

    private async Task<(bool Success, string? RelativePath, string? PhysicalPath, string? Error)> SaveProfilePicture(
        IFormFile file, string[] extensions, long maxBytes)
    {
        if (file.Length == 0 || file.Length > maxBytes)
            return (false, null, null, $"File must be between 1 byte and {maxBytes / 1024 / 1024} MB.");
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!extensions.Contains(extension))
            return (false, null, null, $"Allowed file types: {string.Join(", ", extensions)}.");

        var physicalFolder = Path.Combine(profileUploadRoot, "StudentProfilePicture");
        Directory.CreateDirectory(physicalFolder);
        var fileName = $"{(Student.Id > 0 ? Student.Id.ToString() : "web")}_{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{extension}";
        var physicalPath = Path.Combine(physicalFolder, fileName);
        await using var stream = System.IO.File.Create(physicalPath);
        await file.CopyToAsync(stream);
        return (true, $"/UploadFiles/StudentProfilePicture/{fileName}", physicalPath, null);
    }

    private static string ResolveProfileUploadRoot(IWebHostEnvironment environment, IConfiguration configuration)
    {
        var configuredPath = configuration["StudentUploads:PhysicalRoot"];
        if (string.IsNullOrWhiteSpace(configuredPath))
            configuredPath = Path.Combine("..", "WebAPI", "UploadFiles");

        return Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.GetFullPath(Path.Combine(environment.ContentRootPath, configuredPath));
    }

    private static IEnumerable<SelectListItem> EnumItems<TEnum>() where TEnum : struct, Enum =>
        EnumHelper<TEnum>.GetEnumDropdownList()
            .Select(x => new SelectListItem(x.Text, x.Value.ToString()));
}
