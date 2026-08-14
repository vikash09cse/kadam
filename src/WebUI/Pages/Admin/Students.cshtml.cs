using Core.DTOs;
using Core.Features.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebUI.Services;

namespace WebUI.Pages.Admin
{
    public class StudentsModel(StudentService studentService, AuthenticationService authenticationService, PagePermissionGuard pagePermissions) : PageModel
    {
        public bool IsAdmin { get; private set; }
        public bool CanDelete { get; private set; }

        public async Task OnGetAsync()
        {
            IsAdmin = await studentService.IsAdminUser(authenticationService.GetCurrentUserId());
            CanDelete = await pagePermissions.CanDeleteAsync();
        }

        public async Task<IActionResult> OnGetStudentList(int draw, int start, int length, string? studentName, string? studentId)
        {
            try
            {
                int pageNumber = (start / length) + 1;
                int userId = authenticationService.GetCurrentUserId();

                var result = await studentService.GetStudentList(
                    draw,
                    pageNumber,
                    length,
                    studentName,
                    studentId,
                    userId);

                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = $"An error occurred while fetching the student list. {ex.Message}"
                });
            }
        }

        public async Task<IActionResult> OnPostDeleteStudent(int id)
        {
            var denied = await pagePermissions.ForbidDeleteAsync();
            if (denied != null) return denied;
            var userId = authenticationService.GetCurrentUserId();
            var response = await studentService.DeleteStudent(id, userId);
            return new JsonResult(new { success = response.Success, message = response.Message });
        }
    }
}
