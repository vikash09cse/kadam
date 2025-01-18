using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Core.Features.Admin;
using static Core.Utilities.Enums;
using Core.DTOs.Users;
using Core.Utilities;
using Core.Entities;

namespace WebUI.Pages.Admin
{
    public class PeoplesModel : PageModel
    {
        private readonly AdminService _adminService;
        private readonly AuthenticationService _authenticationService;
        private readonly InstitutionService _institutionService;

        public PeoplesModel(AdminService adminService, AuthenticationService authenticationService, InstitutionService institutionService)
        {
            _adminService = adminService;
            _authenticationService = authenticationService;
            _institutionService = institutionService;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnGetInitialData(int id)
        {
            var roles = await _adminService.GetRolesDropDown();
            var response = new UserInitialDataResponse
            {
                
                Genders = EnumHelper<Gender>.GetEnumDropdownList(),
                Roles = roles,
                ReportRoles = roles,
                UserInfo = new Core.Entities.Users()
            };
            if (id > 0)
            {
                response.UserInfo = await _adminService.GetUser(id);
            }
            return new JsonResult(response);
        }

        public async Task<IActionResult> OnGetUserList(
            int draw,
            int start,
            int length,
            string searchValue)
        {
            int pageNumber = (start / length) + 1;

            var result = await _adminService.GetUserList(
                draw: draw,
                start: pageNumber,
                length: length,
                searchValue: searchValue
            );

            return new JsonResult(result);
        }
        public async Task<IActionResult> OnPostSaveUser([FromBody] Users user)
        {
            try
            {
                if (user == null)
                {
                    return new JsonResult(new { success = false, message = MessageError.InvalidData });
                }
                // Set creation/modification date
                if (user.Id == 0)
                {
                    user.DateCreated = DateTime.UtcNow;
                    user.CreatedBy = _authenticationService.GetCurrentUserId();
                }
                else
                {
                    user.ModifyDate = DateTime.UtcNow;
                    user.ModifyBy = _authenticationService.GetCurrentUserId();
                }

                var result = await _adminService.SaveUser(user);
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = MessageError.ErrorSavingUser(ex.Message)
                });
            }
        }

        public async Task<IActionResult> OnGetUserPrograms(int userId)
        {
            var result = await _adminService.GetUserPrograms(userId);
            return new JsonResult(result);
        }
        public async Task<IActionResult> OnPostSaveUserPrograms([FromBody] List<UserProgram> userPrograms)
        {
            var result = await _adminService.SaveUserPrograms(userPrograms);
            return new JsonResult(result);
        }
        public async Task<IActionResult> OnGetLocationData(int userId)
        {
            var response = new
            {
                InstitutionTypes = EnumHelper<InstitutionType>.GetEnumDropdownList(),
                Divisions = await _adminService.GetDivisionsByStatus(Enums.Status.Active),
                States = await _adminService.GetStatesByStatus(Enums.Status.Active),
                PeopleInstitution = await _adminService.GetPeopleInstitution(userId)
            };

            return new JsonResult(response);
        }
        public async Task<IActionResult> OnGetDistrictListByState(int stateId)
        {
            var districts = await _adminService.GetDistrictsByState(stateId);
            return new JsonResult(districts);
        }
        public async Task<IActionResult> OnGetBlockListByDistrict(int districtId)
        {
            var blocks = await _adminService.GetBlocksByDistrict(districtId);
            return new JsonResult(blocks);
        }

        public async Task<IActionResult> OnGetVillagesByBlock(int blockId)
        {
            var villages = await _adminService.GetVillagesByBlock(blockId);
            return new JsonResult(villages);
        }
        public async Task<IActionResult> OnGetInstitutionsByVillageId(int villageId, int institutionTypeId)
        {
            var institutions = await _institutionService.GetInstitutionsByVillageId(villageId, institutionTypeId);
            return new JsonResult(institutions);
        }
        public async Task<IActionResult> OnPostSavePeopleInstitution([FromBody] PeopleInstitution peopleInstitution)
        {
            var result = await _adminService.SavePeopleInstitution(peopleInstitution);
            return new JsonResult(result);
        }
    }
}
