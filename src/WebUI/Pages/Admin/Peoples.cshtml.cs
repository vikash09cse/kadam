using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Core.Features.Admin;
using Core.Utilities;
using static Core.Utilities.Enums;
using Core.DTOs.Users;
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

        public async Task<IActionResult> OnPostDownloadExcelAsync()
        {
            var userId = _authenticationService.GetCurrentUserId();
            if (userId <= 0)
            {
                return RedirectToPage("/Login");
            }

            try
            {
                var data = await _adminService.GetPeopleExportList();

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Peoples");

                var headers = new[]
                {
                    "Sr. No.",
                    "Full Name",
                    "User Name",
                    "Gender",
                    "Phone",
                    "Alternate Phone",
                    "Email",
                    "Role",
                    "Reportee Role",
                    "Assigned Institutions",
                    "Password (last set)"
                };

                for (int col = 1; col <= headers.Length; col++)
                {
                    worksheet.Cell(1, col).Value = headers[col - 1];
                }

                worksheet.Row(1).Style.Font.Bold = true;
                worksheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;

                int row = 2;
                foreach (var item in data)
                {
                    worksheet.Cell(row, 1).Value = item.SrNo;
                    worksheet.Cell(row, 2).Value = item.FullName;
                    worksheet.Cell(row, 3).Value = item.UserName;
                    worksheet.Cell(row, 4).Value = item.GenderName;
                    worksheet.Cell(row, 5).Value = item.Phone;
                    worksheet.Cell(row, 6).Value = item.AlternatePhone;
                    worksheet.Cell(row, 7).Value = item.Email;
                    worksheet.Cell(row, 8).Value = item.RoleName;
                    worksheet.Cell(row, 9).Value = item.ReporteeRoleName;
                    worksheet.Cell(row, 10).Value = item.AssignedInstitutions;
                    worksheet.Cell(row, 11).Value = item.LastGeneratedPassword ?? string.Empty;
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream, false);
                stream.Position = 0;

                var fileName = $"Peoples_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Unable to generate Excel file. {ex.Message}";
                return RedirectToPage();
            }
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
                response.UserInfo.LastGeneratedPassword = null;
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
        public async Task<IActionResult> OnPostSaveUser([FromBody] UserSaveRequest userRequest)
        {
            try
            {
                if (userRequest?.User == null)
                {
                    return new JsonResult(new { success = false, message = MessageError.InvalidData });
                }

                var user = userRequest.User;
                
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

                var result = await _adminService.SaveUser(user, userRequest.Password, userRequest.AutoGeneratePassword);
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

        public IActionResult OnGetGeneratePasswordPreview()
        {
            return new JsonResult(new { password = PasswordManagement.GenerateSecurePassword() });
        }

        public async Task<IActionResult> OnPostResetUserPassword(int id)
        {
            var result = await _adminService.ResetUserPassword(id);
            return new JsonResult(result);
        }
    }
}
