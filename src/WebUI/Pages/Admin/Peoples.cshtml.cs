using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Core.Features.Admin;
using Core.DTOs;
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

        public PeoplesModel(AdminService adminService, AuthenticationService authenticationService)
        {
            _adminService = adminService;
            _authenticationService = authenticationService;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnGetInitialData(int id)
        {
            var response = new UserInitialDataResponse
            {
                Genders = EnumHelper<Gender>.GetEnumList()
                    .Select(e => new DropdownDTO { Value = ((int)e.Value).ToString(), Text = e.Description })
                    .ToList(),
                Roles = new List<DropdownDTO>
                {
                    new () { Value = "1", Text = "Admin" },
                    new () { Value = "2", Text = "User" }
                },
                ReportRoles = new List<DropdownDTO>
                {
                    new() { Value = "1", Text = "Admin" },
                    new () { Value = "2", Text = "User" }
                },
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
                    user.CreatedBy = _authenticationService.GetUserIdFromToken();
                }
                else
                {
                    user.ModifyDate = DateTime.UtcNow;
                    user.ModifyBy = _authenticationService.GetUserIdFromToken();
                }

                var result = await _adminService.SaveUser(user);
                if(result.StatusCode == AppStatusCodes.Success)
                {
                    return new JsonResult(new
                    {
                        success = true,
                        message = MessageSuccess.Saved,
                        data = result
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = result.Message
                    });
                }
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
    }
}
