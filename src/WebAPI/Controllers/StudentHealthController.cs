using Core.Entities;
using Core.Features.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentHealthController(StudentHealthService studentHealthService) : ControllerBase
    {
        private readonly StudentHealthService _studentHealthService = studentHealthService;

        [HttpPost("SaveStudentHealth")]
        public async Task<IActionResult> SaveStudentHealth([FromBody] StudentHealth health)
        {
            var response = await _studentHealthService.SaveStudentHealth(health);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{id}/{userId}")]
        public async Task<IActionResult> DeleteStudentHealth(int id, int userId)
        {
            var response = await _studentHealthService.DeleteStudentHealth(id, userId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudentHealth(int id)
        {
            var response = await _studentHealthService.GetStudentHealth(id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetStudentHealthByStudentId(int studentId)
        {
            var response = await _studentHealthService.GetStudentHealthByStudentId(studentId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStudentHealths()
        {
            var response = await _studentHealthService.GetAllStudentHealths();
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("default-data/{studentId}")]
        public async Task<IActionResult> GetHealthDefaultData(int studentId)
        {
            var response = await _studentHealthService.GetHealthDefaultData(studentId);
            return StatusCode(response.StatusCode, response);
        }
    }
} 