using Core.Entities;
using Core.Features.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentFollowupController(StudentFollowupService studentFollowupService) : ControllerBase
    {
        private readonly StudentFollowupService _studentFollowupService = studentFollowupService;

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SaveStudentFollowup([FromBody] StudentFollowup studentFollowup)
        {
            var response = await _studentFollowupService.SaveStudentFollowup(studentFollowup);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStudentFollowup(int id)
        {
            var response = await _studentFollowupService.GetStudentFollowup(id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet]
        public async Task<IActionResult> GetStudentFollowupList(
            [FromQuery] int? studentId = null,
            [FromQuery] int? institutionId = null,
            [FromQuery] int? gradeId = null,
            [FromQuery] string section = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int createdBy = 0)
        {
            var response = await _studentFollowupService.GetStudentFollowupList(studentId, institutionId, gradeId, section, fromDate, toDate, createdBy);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("delete/{id}/{userId}")]
        public async Task<IActionResult> DeleteStudentFollowup(int id, int userId)
        {
            var response = await _studentFollowupService.DeleteStudentFollowup(id, userId);
            return StatusCode(response.StatusCode, response);
        }
    }
}
