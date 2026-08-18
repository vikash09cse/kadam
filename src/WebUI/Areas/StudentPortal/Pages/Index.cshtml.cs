using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Areas.StudentPortal.Pages;

public sealed class IndexModel : PageModel
{
    public IActionResult OnGet() => Redirect("/StudentPortal/Dashboard");
}
