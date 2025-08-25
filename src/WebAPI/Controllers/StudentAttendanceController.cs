using Core.DTOs;
using Core.DTOs.App;
using Core.Entities;
using Core.Features.Admin;
using Core.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    // [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentAttendanceController(StudentAttendanceService studentAttendanceService) : ControllerBase
    {
        private readonly StudentAttendanceService _studentAttendanceService = studentAttendanceService;

        [HttpPost]
        public async Task<IActionResult> SaveStudentAttendance([FromBody] StudentAttendance studentAttendance)
        {
            var response = await _studentAttendanceService.SaveStudentAttendance(studentAttendance);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> SaveStudentAttendanceBulk([FromBody] List<StudentAttendanceSaveDTO> attendanceDataJson, [FromQuery] int createdBy)
        {
            var response = await _studentAttendanceService.SaveStudentAttendanceBulk(attendanceDataJson, createdBy);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudentAttendance(int id)
        {
            var response = await _studentAttendanceService.GetStudentAttendance(id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetStudentAttendanceList(
            [FromQuery] int? institutionId = null,
            [FromQuery] int? gradeId = null,
            [FromQuery] string section = null,
            [FromQuery] DateTime? attendanceDate = null,
            [FromQuery] int createdBy = 0)
        {
            var response = await _studentAttendanceService.GetStudentAttendanceList(institutionId, gradeId, section, attendanceDate, createdBy);
            return StatusCode(response.StatusCode, response);
        }
    }
}
