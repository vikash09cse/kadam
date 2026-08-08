using Core.Features.StudentsWeb;
using Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebUI.Areas.StudentPortal.Pages;

public sealed class DocumentsModel(
    StudentsWebService studentsService,
    AuthenticationService authenticationService,
    IWebHostEnvironment environment,
    IConfiguration configuration)
    : StudentPortalPageModel(studentsService, authenticationService)
{
    private readonly string uploadRoot = ResolveUploadRoot(environment, configuration);
    private readonly string publicBaseUrl = configuration["StudentUploads:PublicBaseUrl"]?.TrimEnd('/') ?? string.Empty;

    [BindProperty]
    public StudentsWebDocumentDTO Document { get; set; } = new();

    [BindProperty]
    public IFormFile? DocumentFile { get; set; }

    public IReadOnlyList<StudentsWebDocumentDTO> Documents { get; private set; } = [];
    public string StudentName { get; private set; } = string.Empty;
    public IEnumerable<SelectListItem> DocumentTypes =>
        EnumHelper<Enums.DocumentTypes>.GetEnumDropdownList()
            .Select(x => new SelectListItem(x.Text, x.Value.ToString()));
    public string GetDocumentUrl(string? path) =>
        string.IsNullOrWhiteSpace(path) ? string.Empty : $"{publicBaseUrl}{path}";

    public async Task<IActionResult> OnGetAsync(int studentId, int editId = 0)
    {
        var denied = await RequirePageAsync("/StudentPortal/Documents");
        if (denied is not null) return denied;
        if (studentId <= 0) return Redirect("/StudentPortal/Directory");
        await Load(studentId);
        Document = editId > 0
            ? Documents.FirstOrDefault(x => x.Id == editId) ?? new StudentsWebDocumentDTO { StudentId = studentId }
            : new StudentsWebDocumentDTO { StudentId = studentId };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var denied = await RequirePageAsync("/StudentPortal/Documents");
        if (denied is not null) return denied;

        if (Document.Id > 0)
        {
            var existingDocuments = await StudentsService.GetDocuments(Document.StudentId, CurrentUserId);
            var existing = existingDocuments.FirstOrDefault(x => x.Id == Document.Id);
            if (existing is null) return NotFound();
            Document.DocumentPath = existing.DocumentPath;
            Document.DocumentFileName = existing.DocumentFileName;
        }
        else
        {
            // Never trust paths posted for a new document.
            Document.DocumentPath = null;
            Document.DocumentFileName = null;
        }

        string? newFile = null;
        if (DocumentFile is not null)
        {
            var extension = Path.GetExtension(DocumentFile.FileName).ToLowerInvariant();
            if (DocumentFile.Length == 0 || DocumentFile.Length > 10 * 1024 * 1024 ||
                !new[] { ".pdf", ".jpg", ".jpeg", ".png" }.Contains(extension))
            {
                ModelState.AddModelError(nameof(DocumentFile), "Document must be a PDF or image up to 10 MB.");
                await Load(Document.StudentId);
                return Page();
            }
            var folder = Path.Combine(uploadRoot, "Documents");
            Directory.CreateDirectory(folder);
            var fileName = $"{Document.StudentId}_{Guid.NewGuid():N}{extension}";
            newFile = Path.Combine(folder, fileName);
            await using var stream = System.IO.File.Create(newFile);
            await DocumentFile.CopyToAsync(stream);
            Document.DocumentPath = $"/UploadFiles/Documents/{fileName}";
            Document.DocumentFileName = Path.GetFileName(DocumentFile.FileName);
        }

        var result = await StudentsService.SaveDocument(Document, CurrentUserId);
        if (!result.Success)
        {
            if (newFile is not null && System.IO.File.Exists(newFile)) System.IO.File.Delete(newFile);
            if (Document.Id == 0)
            {
                Document.DocumentPath = null;
                Document.DocumentFileName = null;
            }
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            await Load(Document.StudentId);
            return Page();
        }
        TempData["SuccessMessage"] = result.Message;
        return Redirect("/StudentPortal/Documents?studentId=" + Document.StudentId);
    }

    public async Task<IActionResult> OnPostDeleteAsync(int documentId, int studentId)
    {
        var denied = await RequirePageAsync("/StudentPortal/Documents");
        if (denied is not null) return denied;
        var result = await StudentsService.DeleteDocument(documentId, CurrentUserId);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return Redirect("/StudentPortal/Documents?studentId=" + studentId);
    }

    private async Task Load(int studentId)
    {
        Documents = await StudentsService.GetDocuments(studentId, CurrentUserId);
        var student = await StudentsService.GetStudent(studentId, CurrentUserId);
        StudentName = student is null ? string.Empty : $"{student.FirstName} {student.LastName}".Trim();
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
