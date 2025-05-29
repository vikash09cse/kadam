using Core.Features.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Core.DTOs.App;
using static Core.Utilities.DBConstant;

namespace WebAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentBaselineDetailController(StudentBaselineDetailService studentBaselineDetailService) : ControllerBase
    {
        private readonly StudentBaselineDetailService _studentBaselineDetailService = studentBaselineDetailService;

        
        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetStudentBaselineDetailWithSubjects(int studentId)
        {
            string baselineType = BaselineType.BaselinePreAssessment;
            var response = await _studentBaselineDetailService.GetStudentBaselineDetailWithSubjects(studentId, baselineType);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("save-student-baseline")]
        public async Task<IActionResult> SaveStudentBaselineDetail([FromBody] StudentBaselineDetailWithSubjectSaveDTO entity)
        {
            string baselineType = BaselineType.BaselinePreAssessment;
            entity.BaselineType = baselineType;
            var response = await _studentBaselineDetailService.SaveStudentBaselineDetail(entity);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("studentEndBaseline/{studentId}")]
        public async Task<IActionResult> GetStudentEndBaselineDetailWithSubjects(int studentId)
        {
            string baselineType = BaselineType.EndlinePreAssessment;
            var response = await _studentBaselineDetailService.GetStudentBaselineDetailWithSubjects(studentId, baselineType);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("save-student-endBaseline")]
        public async Task<IActionResult> SaveStudentEndBaselineDetail([FromBody] StudentBaselineDetailWithSubjectSaveDTO entity)
        {
            string baselineType = BaselineType.EndlinePreAssessment;
            entity.BaselineType = baselineType;
            var response = await _studentBaselineDetailService.SaveStudentBaselineDetail(entity);
            return StatusCode(response.StatusCode, response);
        }
    }
} 