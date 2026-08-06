using Core.Features.Admin;
using Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages.Admin
{
    public class AssignDivisionModel : PageModel
    {
        private readonly AdminService _adminService;
        private readonly AuthenticationService _authenticationService;

        public AssignDivisionModel(AdminService adminService, AuthenticationService authenticationService)
        {
            _adminService = adminService;
            _authenticationService = authenticationService;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty]
        public List<int> SelectedDivisionIds { get; set; } = [];

        public string UserFullName { get; private set; } = string.Empty;
        public string RoleName { get; private set; } = string.Empty;
        public IEnumerable<Core.DTOs.DropdownDTO> Divisions { get; private set; } = [];

        public async Task<IActionResult> OnGetAsync()
        {
            var redirect = EnsureAuthenticated();
            if (redirect != null)
            {
                return redirect;
            }

            if (Id <= 0)
            {
                return RedirectToPage("/Admin/Peoples");
            }

            var loaded = await LoadPageDataAsync();
            if (!loaded)
            {
                return RedirectToPage("/Admin/Peoples");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            var redirect = EnsureAuthenticated();
            if (redirect != null)
            {
                return redirect;
            }

            if (Id <= 0)
            {
                return RedirectToPage("/Admin/Peoples");
            }

            if (SelectedDivisionIds == null || SelectedDivisionIds.Count == 0)
            {
                TempData["ErrorMessage"] = "Please select at least one division.";
                await LoadPageDataAsync();
                return Page();
            }

            var result = await _adminService.SavePeopleDivisions(Id, SelectedDivisionIds);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Divisions assigned successfully.";
                return RedirectToPage(new { id = Id });
            }

            TempData["ErrorMessage"] = result.Message ?? "Unable to save division assignment.";
            await LoadPageDataAsync();
            return Page();
        }

        private async Task<bool> LoadPageDataAsync()
        {
            var user = await _adminService.GetUser(Id);
            if (user == null || user.Id <= 0)
            {
                return false;
            }

            UserFullName = $"{user.FirstName} {user.LastName}".Trim();
            Divisions = await _adminService.GetDivisionsByStatus(Enums.Status.Active);

            var role = await _adminService.GetRole(user.RoleId);
            RoleName = role?.RoleName ?? string.Empty;

            if (!RoleNames.IsDivisionScopedRole(role))
            {
                TempData["ErrorMessage"] = "Division assignment is only available for roles with Allow Multiple Division enabled.";
                return false;
            }

            if (SelectedDivisionIds == null || SelectedDivisionIds.Count == 0)
            {
                SelectedDivisionIds = (await _adminService.GetPeopleDivisionIds(Id)).ToList();
            }

            return true;
        }

        private IActionResult? EnsureAuthenticated()
        {
            var userId = _authenticationService.GetCurrentUserId();
            if (userId <= 0)
            {
                return RedirectToPage("/Login");
            }

            return null;
        }
    }
}
