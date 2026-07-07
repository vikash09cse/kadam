using Core.DTOs;
using Core.Entities;
using Core.Features.Admin;
using Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages.Admin
{
    public class AssignInstitutionModel : PageModel
    {
        private readonly AdminService _adminService;
        private readonly AuthenticationService _authenticationService;
        private readonly InstitutionService _institutionService;

        public AssignInstitutionModel(
            AdminService adminService,
            AuthenticationService authenticationService,
            InstitutionService institutionService)
        {
            _adminService = adminService;
            _authenticationService = authenticationService;
            _institutionService = institutionService;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty]
        public List<int> SelectedInstitutionIds { get; set; } = [];

        public string UserFullName { get; private set; } = string.Empty;
        public string RoleName { get; private set; } = string.Empty;
        public int CurrentDivisionId { get; private set; }
        public int CurrentStateId { get; private set; }
        public int CurrentDistrictId { get; private set; }
        public int CurrentBlockId { get; private set; }
        public int CurrentVillageId { get; private set; }
        public int CurrentInstitutionTypeId { get; private set; }
        public string DivisionName { get; private set; } = string.Empty;
        public string StateName { get; private set; } = string.Empty;
        public IEnumerable<DropdownDTO> InstitutionTypes { get; private set; } = [];
        public List<InstitutionAssignmentRow> AssignedInstitutions { get; private set; } = [];

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

            await LoadPageDataAsync();
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

            if (SelectedInstitutionIds.Count == 0)
            {
                TempData["ErrorMessage"] = "Please select at least one institution.";
                await LoadPageDataAsync();
                return Page();
            }

            var request = new PeopleInstitution
            {
                UserId = Id,
                InstitutionIds = string.Join(",", SelectedInstitutionIds.Distinct())
            };

            var result = await _adminService.SavePeopleInstitution(request);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Institutions assigned successfully.";
                return RedirectToPage(new { id = Id });
            }

            TempData["ErrorMessage"] = result.Message;
            await LoadPageDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnGetDistrictListByDivision(int divisionId, int stateId)
        {
            return new JsonResult(await _adminService.GetDistrictsByDivisionId(divisionId, stateId));
        }

        public async Task<IActionResult> OnGetBlockListByDivision(int divisionId, int districtId)
        {
            return new JsonResult(await _adminService.GetBlocksByDivisionId(divisionId, districtId));
        }

        public async Task<IActionResult> OnGetVillageListByDivision(int divisionId, int blockId)
        {
            return new JsonResult(await _adminService.GetVillagesByDivisionId(divisionId, blockId));
        }

        public async Task<IActionResult> OnGetInstitutionsByVillageId(int villageId, int institutionTypeId)
        {
            return new JsonResult(await _institutionService.GetInstitutionsByVillageId(villageId, institutionTypeId));
        }

        private IActionResult? EnsureAuthenticated()
        {
            var currentUserId = _authenticationService.GetCurrentUserId();
            return currentUserId <= 0 ? RedirectToPage("/Login") : null;
        }

        private async Task LoadPageDataAsync()
        {
            var user = await _adminService.GetUser(Id);
            if (user == null || user.Id <= 0)
            {
                AssignedInstitutions = [];
                return;
            }

            UserFullName = $"{user.FirstName} {user.LastName}".Trim();
            InstitutionTypes = EnumHelper<Enums.InstitutionType>.GetEnumDropdownList();

            var roles = await _adminService.GetRolesDropDown();
            RoleName = roles.FirstOrDefault(x => x.Value == user.RoleId)?.Text ?? string.Empty;

            var assignedInstitutionIds = new HashSet<int>();
            var peopleInstitution = await _adminService.GetPeopleInstitution(Id);
            if (peopleInstitution == null)
            {
                AssignedInstitutions = [];
                return;
            }

            if (!string.IsNullOrWhiteSpace(peopleInstitution.InstitutionIds))
            {
                assignedInstitutionIds = peopleInstitution.InstitutionIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(value => int.TryParse(value, out var institutionId) ? institutionId : 0)
                    .Where(value => value > 0)
                    .ToHashSet();
            }

            if (SelectedInstitutionIds.Count == 0)
            {
                SelectedInstitutionIds = assignedInstitutionIds.ToList();
            }

            await LoadLocationNamesAsync(peopleInstitution);
            await LoadAssignedInstitutionsAsync(SelectedInstitutionIds);
        }

        private async Task LoadLocationNamesAsync(PeopleInstitution peopleInstitution)
        {
            CurrentDivisionId = peopleInstitution.DivisionId;
            CurrentStateId = peopleInstitution.StateId;
            CurrentDistrictId = peopleInstitution.DistrictId;
            CurrentBlockId = peopleInstitution.BlockId;
            CurrentVillageId = peopleInstitution.VillageId;
            CurrentInstitutionTypeId = peopleInstitution.InstitutionTypeId;

            DivisionName = (await _adminService.GetDivisionsByStatus(Enums.Status.Active))
                .FirstOrDefault(x => x.Value == peopleInstitution.DivisionId)?.Text ?? string.Empty;

            StateName = (await _adminService.GetStatesByDivisionId(peopleInstitution.DivisionId))
                .FirstOrDefault(x => x.Value == peopleInstitution.StateId)?.Text ?? string.Empty;
        }

        private async Task LoadAssignedInstitutionsAsync(IEnumerable<int> institutionIds)
        {
            var ids = institutionIds.Distinct().ToList();
            AssignedInstitutions = [];

            foreach (var institutionId in ids)
            {
                var institution = await _institutionService.GetInstitutionById(institutionId);
                if (institution == null)
                {
                    continue;
                }

                AssignedInstitutions.Add(new InstitutionAssignmentRow
                {
                    Id = institution.Id,
                    DivisionName = DivisionName,
                    StateName = institution.StateName,
                    DistrictName = institution.DistrictName,
                    BlockName = institution.BlockName,
                    VillageName = institution.VillageName,
                    InstitutionName = institution.InstitutionName
                });
            }
        }

        public class InstitutionAssignmentRow
        {
            public int Id { get; set; }
            public string DivisionName { get; set; } = string.Empty;
            public string StateName { get; set; } = string.Empty;
            public string DistrictName { get; set; } = string.Empty;
            public string BlockName { get; set; } = string.Empty;
            public string VillageName { get; set; } = string.Empty;
            public string InstitutionName { get; set; } = string.Empty;
        }
    }
}
