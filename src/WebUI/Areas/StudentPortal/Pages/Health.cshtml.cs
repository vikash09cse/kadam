using Core.Features.StudentsWeb;
using Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebUI.Areas.StudentPortal.Pages;

public sealed class HealthModel(
    StudentsWebService studentsService,
    AuthenticationService authenticationService,
    IWebHostEnvironment environment,
    IConfiguration configuration)
    : StudentPortalPageModel(studentsService, authenticationService)
{
    private readonly string uploadRoot = ResolveUploadRoot(environment, configuration);
    private readonly string publicBaseUrl = configuration["StudentUploads:PublicBaseUrl"]?.TrimEnd('/') ?? string.Empty;

    [BindProperty]
    public StudentsWebHealthDTO Health { get; set; } = new();

    [BindProperty]
    public IFormFile? Certificate { get; set; }

    public IEnumerable<SelectListItem> ChallengeTypes =>
        EnumHelper<Enums.PhysicalChallengedTypes>.GetEnumDropdownList()
            .Select(x => new SelectListItem(x.Text, x.Value.ToString()));
    public string CertificateUrl => string.IsNullOrWhiteSpace(Health.DisabilityCertificatePath)
        ? string.Empty
        : $"{publicBaseUrl}{Health.DisabilityCertificatePath}";

    public async Task<IActionResult> OnGetAsync(int studentId)
    {
        var denied = await RequirePageAsync("/StudentPortal/Health");
        if (denied is not null) return denied;
        if (studentId <= 0) return Redirect("/StudentPortal/Directory");
        Health = await StudentsService.GetHealth(studentId, CurrentUserId)
            ?? new StudentsWebHealthDTO { StudentId = studentId };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var denied = await RequirePageAsync("/StudentPortal/Health");
        if (denied is not null) return denied;

        var existing = await StudentsService.GetHealth(Health.StudentId, CurrentUserId);
        if (existing is null) return NotFound();
        Health.Id = existing.Id;
        Health.StudentName = existing.StudentName;
        Health.DisabilityCertificatePath = existing.DisabilityCertificatePath;
        Health.DisabilityCertificateFileName = existing.DisabilityCertificateFileName;

        string? newFile = null;
        if (Health.PhysicallyChallenged && Certificate is not null)
        {
            var extension = Path.GetExtension(Certificate.FileName).ToLowerInvariant();
            if (Certificate.Length == 0 || Certificate.Length > 10 * 1024 * 1024 ||
                !new[] { ".pdf", ".jpg", ".jpeg", ".png" }.Contains(extension))
            {
                ModelState.AddModelError(nameof(Certificate), "Certificate must be a PDF or image up to 10 MB.");
                return Page();
            }
            var folder = Path.Combine(uploadRoot, "HealthCertificates");
            Directory.CreateDirectory(folder);
            var fileName = $"{Health.StudentId}_{Guid.NewGuid():N}{extension}";
            newFile = Path.Combine(folder, fileName);
            await using var stream = System.IO.File.Create(newFile);
            await Certificate.CopyToAsync(stream);
            Health.DisabilityCertificatePath = $"/UploadFiles/HealthCertificates/{fileName}";
            Health.DisabilityCertificateFileName = Path.GetFileName(Certificate.FileName);
        }

        var result = await StudentsService.SaveHealth(Health, CurrentUserId);
        if (!result.Success)
        {
            if (newFile is not null && System.IO.File.Exists(newFile)) System.IO.File.Delete(newFile);
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            return Page();
        }
        TempData["SuccessMessage"] = result.Message;
        return Redirect("/StudentPortal/Health?studentId=" + Health.StudentId);
    }

    private static string ResolveUploadRoot(IWebHostEnvironment environment, IConfiguration configuration)
    {
        var configuredPath = configuration["StudentUploads:PhysicalRoot"];
        if (string.IsNullOrWhiteSpace(configuredPath))
            configuredPath = Path.Combine("..", "WebAPI", "UploadFiles");

        return Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.GetFullPath(Path.Combine(environment.ContentRootPath, configuredPath));
    }
}
