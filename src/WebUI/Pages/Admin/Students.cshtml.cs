using Core.DTOs;
using Core.Features.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages.Admin
{
    public class StudentsModel(StudentService studentService, AuthenticationService authenticationService) : PageModel
    {
        public bool IsAdmin { get; private set; }

        public async Task OnGetAsync()
        {
            IsAdmin = await studentService.IsAdminUser(authenticationService.GetCurrentUserId());
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
            catch (Exception)
            {
                return new JsonResult(new { success = false, message = "An error occurred while fetching the student list." });
            }
        }

        public async Task<IActionResult> OnPostDeleteStudent(int id)
        {
            var userId = authenticationService.GetCurrentUserId();
            var response = await studentService.DeleteStudent(id, userId);
            return new JsonResult(new { success = response.Success, message = response.Message });
        }
    }
}
