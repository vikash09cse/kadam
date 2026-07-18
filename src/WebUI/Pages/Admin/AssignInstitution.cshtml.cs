using Core.DTOs;
using Core.Entities;
using Core.Features.Admin;
using Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace WebUI.Pages.Admin
{
    public class AssignInstitutionModel : PageModel
    {
        private readonly AdminService _adminService;
        private readonly AuthenticationService _authenticationService;
        private readonly InstitutionService _institutionService;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

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

        [BindProperty]
        public string AssignmentsJson { get; set; } = "[]";

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
        public IEnumerable<DropdownDTO> Divisions { get; private set; } = [];
        public IEnumerable<DropdownDTO> States { get; private set; } = [];
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

            var assignments = DeserializeAssignments(AssignmentsJson);
            if (assignments.Count == 0 && SelectedInstitutionIds.Count > 0)
            {
                assignments = SelectedInstitutionIds.Distinct()
                    .Select(institutionId => new PeopleInstitutionAssignmentDTO { InstitutionId = institutionId })
                    .ToList();
            }

            if (assignments.Count == 0)
            {
                TempData["ErrorMessage"] = "Please select at least one institution.";
                await LoadPageDataAsync();
                return Page();
            }

            foreach (var assignment in assignments)
            {
                if (assignment.GradeSections == null
                    || assignment.GradeSections.Count == 0
                    || assignment.GradeSections.All(x => string.IsNullOrWhiteSpace(x.Sections)))
                {
                    TempData["ErrorMessage"] = "Please select at least one grade and section for each assigned institution.";
                    SelectedInstitutionIds = assignments.Select(x => x.InstitutionId).ToList();
                    await LoadPageDataAsync();
                    return Page();
                }
            }

            var gradeAndSectionByInstitutionId = assignments.ToDictionary(
                x => x.InstitutionId,
                x => JsonSerializer.Serialize(x.GradeSections.Select(gs => new
                {
                    gs.GradeId,
                    gs.Sections
                })));

            var request = new PeopleInstitution
            {
                UserId = Id,
                InstitutionIds = string.Join(",", assignments.Select(x => x.InstitutionId).Distinct())
            };

            var result = await _adminService.SavePeopleInstitution(request, gradeAndSectionByInstitutionId);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Institutions assigned successfully.";
                return RedirectToPage(new { id = Id });
            }

            TempData["ErrorMessage"] = result.Message;
            SelectedInstitutionIds = assignments.Select(x => x.InstitutionId).ToList();
            await LoadPageDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnGetDistrictListByDivision(int divisionId, int stateId)
        {
            return new JsonResult(await _adminService.GetDistrictsByDivisionId(divisionId, stateId));
        }

        public async Task<IActionResult> OnGetStateListByDivision(int divisionId)
        {
            return new JsonResult(await _adminService.GetStatesByDivisionId(divisionId));
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
            return new JsonResult(await _institutionService.GetInstitutionsWithGradeSectionsByVillageId(villageId, institutionTypeId));
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
            Divisions = await _adminService.GetDivisionsByStatus(Enums.Status.Active);
            InstitutionTypes = EnumHelper<Enums.InstitutionType>.GetEnumDropdownList();

            var roles = await _adminService.GetRolesDropDown();
            RoleName = roles.FirstOrDefault(x => x.Value == user.RoleId)?.Text ?? string.Empty;

            CurrentDivisionId = user.DivisionId ?? 0;
            if (CurrentDivisionId > 0)
            {
                States = await _adminService.GetStatesByDivisionId(CurrentDivisionId);
                CurrentStateId = States.Select(x => x.Value).FirstOrDefault();
            }

            DivisionName = Divisions.FirstOrDefault(x => x.Value == CurrentDivisionId)?.Text ?? string.Empty;
            StateName = States.FirstOrDefault(x => x.Value == CurrentStateId)?.Text ?? string.Empty;

            var assignmentRows = (await _adminService.GetPeopleInstitutionAssignments(Id)).ToList();
            if (assignmentRows.Count == 0)
            {
                AssignedInstitutions = [];
                return;
            }

            var peopleInstitution = await _adminService.GetPeopleInstitution(Id);
            if (peopleInstitution != null)
            {
                await LoadLocationNamesAsync(peopleInstitution);
            }

            if (SelectedInstitutionIds.Count == 0)
            {
                SelectedInstitutionIds = assignmentRows
                    .Select(row => int.TryParse(row.InstitutionIds, out var institutionId) ? institutionId : 0)
                    .Where(id => id > 0)
                    .Distinct()
                    .ToList();
            }

            await LoadAssignedInstitutionsAsync(assignmentRows);
        }

        private async Task LoadLocationNamesAsync(PeopleInstitution peopleInstitution)
        {
            CurrentDivisionId = peopleInstitution.DivisionId;
            CurrentStateId = peopleInstitution.StateId;
            CurrentDistrictId = peopleInstitution.DistrictId;
            CurrentBlockId = peopleInstitution.BlockId;
            CurrentVillageId = peopleInstitution.VillageId;
            CurrentInstitutionTypeId = peopleInstitution.InstitutionTypeId;

            if (CurrentDivisionId > 0)
            {
                States = await _adminService.GetStatesByDivisionId(CurrentDivisionId);
            }

            DivisionName = Divisions.FirstOrDefault(x => x.Value == CurrentDivisionId)?.Text ?? string.Empty;
            StateName = States.FirstOrDefault(x => x.Value == CurrentStateId)?.Text ?? string.Empty;
        }

        private async Task LoadAssignedInstitutionsAsync(List<PeopleInstitution> assignmentRows)
        {
            AssignedInstitutions = [];
            var gradeCatalog = (await _adminService.GetGradesAndSections())
                .ToDictionary(x => x.Id, x => x.GradeName);

            foreach (var row in assignmentRows)
            {
                if (!int.TryParse(row.InstitutionIds, out var institutionId) || institutionId <= 0)
                {
                    continue;
                }

                var institution = await _institutionService.GetInstitutionById(institutionId);
                if (institution == null)
                {
                    continue;
                }

                var gradeSections = ParseGradeSections(row.GradeAndSection);
                foreach (var item in gradeSections)
                {
                    if (gradeCatalog.TryGetValue(item.GradeId, out var gradeName))
                    {
                        item.GradeName = gradeName;
                    }
                    else if (string.IsNullOrWhiteSpace(item.GradeName))
                    {
                        item.GradeName = $"Grade {item.GradeId}";
                    }
                }

                AssignedInstitutions.Add(new InstitutionAssignmentRow
                {
                    Id = institution.Id,
                    DivisionId = institution.DivisionId,
                    StateId = institution.StateId,
                    DistrictId = institution.DistrictId,
                    BlockId = institution.BlockId,
                    VillageId = institution.VillageId,
                    InstitutionTypeId = institution.InstitutionType,
                    DivisionName = Divisions.FirstOrDefault(x => x.Value == institution.DivisionId)?.Text ?? DivisionName,
                    StateName = institution.StateName,
                    DistrictName = institution.DistrictName,
                    BlockName = institution.BlockName,
                    VillageName = institution.VillageName,
                    InstitutionName = institution.InstitutionName,
                    GradeSectionSummary = FormatGradeSectionSummary(gradeSections),
                    GradeSections = gradeSections
                });
            }
        }

        private static List<PeopleGradeSectionDTO> ParseGradeSections(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return [];
            }

            try
            {
                return JsonSerializer.Deserialize<List<PeopleGradeSectionDTO>>(json, JsonOptions) ?? [];
            }
            catch
            {
                return [];
            }
        }

        private static string FormatGradeSectionSummary(IEnumerable<PeopleGradeSectionDTO> gradeSections)
        {
            var parts = gradeSections
                .Where(x => x.GradeId > 0 && !string.IsNullOrWhiteSpace(x.Sections))
                .Select(x =>
                {
                    var gradeLabel = string.IsNullOrWhiteSpace(x.GradeName) ? $"Grade {x.GradeId}" : x.GradeName;
                    return $"{gradeLabel} ({x.Sections})";
                })
                .ToList();

            return parts.Count == 0 ? string.Empty : string.Join(", ", parts);
        }

        private static List<PeopleInstitutionAssignmentDTO> DeserializeAssignments(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return [];
            }

            try
            {
                return JsonSerializer.Deserialize<List<PeopleInstitutionAssignmentDTO>>(json, JsonOptions) ?? [];
            }
            catch
            {
                return [];
            }
        }

        public class InstitutionAssignmentRow
        {
            public int Id { get; set; }
            public int DivisionId { get; set; }
            public int StateId { get; set; }
            public int DistrictId { get; set; }
            public int BlockId { get; set; }
            public int VillageId { get; set; }
            public int InstitutionTypeId { get; set; }
            public string DivisionName { get; set; } = string.Empty;
            public string StateName { get; set; } = string.Empty;
            public string DistrictName { get; set; } = string.Empty;
            public string BlockName { get; set; } = string.Empty;
            public string VillageName { get; set; } = string.Empty;
            public string InstitutionName { get; set; } = string.Empty;
            public string GradeSectionSummary { get; set; } = string.Empty;
            public List<PeopleGradeSectionDTO> GradeSections { get; set; } = [];
        }
    }
}
