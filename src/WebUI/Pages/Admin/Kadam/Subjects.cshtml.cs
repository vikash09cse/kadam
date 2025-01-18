using Core.DTOs;
using Core.Entities;
using Core.Features.Admin;
using Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages.Admin.Kadam
{
    public class SubjectsModel : PageModel
    {
        private readonly AdminService _adminService;
        private readonly AuthenticationService _authenticationService;

        public SubjectsModel(AdminService adminService, AuthenticationService authenticationService)
        {
            _adminService = adminService;
            _authenticationService = authenticationService;
        }

        public async Task<IActionResult> OnGetSubjectList(int draw, int start, int length, string searchValue)
        {
            var result = await _adminService.GetSubjects(draw, start, length, searchValue);
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnPostSaveSubject([FromBody] Subject subject)
        {
            if (subject == null)
            {
                return new JsonResult(new { success = false, message = MessageError.InvalidData });
            }
            
            var response = await _adminService.SaveSubject(subject, _authenticationService.GetCurrentUserId());
            return new JsonResult(response);
        }

        public async Task<IActionResult> OnPostDeleteSubject(int id)
        {
            var response = await _adminService.DeleteSubject(id, _authenticationService.GetCurrentUserId());
            return new JsonResult(response);
        }

        public async Task<IActionResult> OnGetSubjectDetail(int id)
        {
            var response = await _adminService.GetSubject(id);
            return new JsonResult(response);
        }
    }
}
