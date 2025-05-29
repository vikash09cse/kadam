using Core.Features.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Core.DTOs;
using Core.Entities;
using Core.DTOs.App;

namespace WebAPI.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentProgressController(StudentProgressService studentProgressService) : ControllerBase
    {
        private readonly StudentProgressService _studentProgressService = studentProgressService;

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetStudentProgressDetail(int studentId)
        {
            var response = await _studentProgressService.GetStudentProgressDetail(studentId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost("saveStudentProgressStep")]
        public async Task<IActionResult> SaveStudentProgress([FromBody] StudentProgressStep studentProgress)
        {
            var response = await _studentProgressService.SaveStudentProgress(studentProgress);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost("saveStudentGradeTestDetail")]
        public async Task<IActionResult> SaveStudentGradeTestDetail([FromBody] StudentGradeTestDetailSaveDTO studentGradeTestDetail)
        {
            var response = await _studentProgressService.SaveStudentGradeTestDetail(studentGradeTestDetail);
            return StatusCode(response.StatusCode, response);
        }
    }
} 