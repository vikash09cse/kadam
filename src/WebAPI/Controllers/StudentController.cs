using Core.Entities;
using Core.Features.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController(StudentService studentService) : ControllerBase
    {
        private readonly StudentService _StudentService = studentService;
        [HttpPost]
        public async Task<IActionResult> SaveStudent([FromBody] Student student)
        {
            var response = await _StudentService.SaveStudent(student);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{id}/{userId}")]
        public async Task<IActionResult> DeleteStudent(int id, int userId)
        {
            var response = await _StudentService.DeleteStudent(id, userId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudent(int id)
        {
            var student = await _StudentService.GetStudent(id);
            return Ok(student);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStudents()
        {
            var students = await _StudentService.GetAllStudents();
            return Ok(students);
        }

        [HttpGet("institutions/{userId}")]
        public async Task<IActionResult> GetInstitutionsByUserId(int userId)
        {
            var institutions = await _StudentService.GetInstitutionsByUserId(userId);
            return Ok(institutions);
        }

        [HttpGet("GetStudentList/{createdBy}")]
        public async Task<IActionResult> GetStudentListMobile(int createdBy)
        {
            var students = await _StudentService.GetStudentListMobile(createdBy);
            return Ok(students);
        }

        [HttpGet("GetStudentListMyInstitution")]
        public async Task<IActionResult> GetStudentListMyInstitutionMobile(
            [FromQuery] int institutionId, 
            [FromQuery] int? gradeId = null, 
            [FromQuery] string section = null, 
            [FromQuery] DateTime? fromDate = null, 
            [FromQuery] DateTime? toDate = null, 
            [FromQuery] int createdBy = 0)
        {
            var students = await _StudentService.GetStudentListMyInstitutionMobile(institutionId, gradeId, section, fromDate, toDate, createdBy);
            return Ok(students);
        }

        [HttpGet("defaultdata/{userId}")]
        public async Task<IActionResult> GetStudentDefaultData(int userId)
        {
            var studentDefaultData = await _StudentService.GetStudentDefaultData(userId);
            return Ok(studentDefaultData);
        }
    }
}
