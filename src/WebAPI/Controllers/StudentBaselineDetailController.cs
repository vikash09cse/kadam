using Core.Features.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Core.DTOs.App;

namespace WebAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentBaselineDetailController(StudentBaselineDetailService studentBaselineDetailService) : ControllerBase
    {
        private readonly StudentBaselineDetailService _studentBaselineDetailService = studentBaselineDetailService;

        // [HttpGet]
        // public async Task<IActionResult> GetAll()
        // {
        //     var response = await _studentBaselineDetailService.GetAllAsync();
        //     return StatusCode(response.StatusCode, response);
        // }

        // [HttpGet("{id}")]
        // public async Task<IActionResult> GetById(int id)
        // {
        //     var response = await _studentBaselineDetailService.GetByIdAsync(id);
        //     return StatusCode(response.StatusCode, response);
        // }

        // [HttpGet("student/{studentId}")]
        // public async Task<IActionResult> GetByStudentId(int studentId)
        // {
        //     var response = await _studentBaselineDetailService.GetByStudentIdAsync(studentId);
        //     return StatusCode(response.StatusCode, response);
        // }

        // [HttpPost]
        // public async Task<IActionResult> SaveStudentBaselineDetail([FromBody] StudentBaselineDetail entity)
        // {
        //     var response = await _studentBaselineDetailService.SaveStudentBaselineDetail(entity);
        //     return StatusCode(response.StatusCode, response);
        // }

        // [HttpDelete("{id}/{deletedBy}")]
        // public async Task<IActionResult> Delete(int id, int deletedBy)
        // {
        //     var response = await _studentBaselineDetailService.DeleteAsync(id, deletedBy);
        //     return StatusCode(response.StatusCode, response);
        // }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetStudentBaselineDetailWithSubjects(int studentId)
        {
            var response = await _studentBaselineDetailService.GetStudentBaselineDetailWithSubjects(studentId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("save-student-baseline")]
        public async Task<IActionResult> SaveStudentBaselineDetail([FromBody] StudentBaselineDetailWithSubjectSaveDTO entity)
        {
            var response = await _studentBaselineDetailService.SaveStudentBaselineDetail(entity);
            return StatusCode(response.StatusCode, response);
        }
    }
} 