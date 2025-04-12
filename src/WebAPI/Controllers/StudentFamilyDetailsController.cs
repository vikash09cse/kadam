using Core.Entities;
using Core.Features.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentFamilyDetailsController(StudentFamilyDetailsService studentFamilyDetailsService) : ControllerBase
    {
        private readonly StudentFamilyDetailsService _studentFamilyDetailsService = studentFamilyDetailsService;

        [HttpPost]
        public async Task<IActionResult> SaveStudentFamilyDetails([FromBody] StudentFamilyDetail familyDetails)
        {
            var response = await _studentFamilyDetailsService.SaveStudentFamilyDetails(familyDetails);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{id}/{userId}")]
        public async Task<IActionResult> DeleteStudentFamilyDetails(int id, int userId)
        {
            var response = await _studentFamilyDetailsService.DeleteStudentFamilyDetails(id, userId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudentFamilyDetails(int id)
        {
            var familyDetails = await _studentFamilyDetailsService.GetStudentFamilyDetails(id);
            return Ok(familyDetails);
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetStudentFamilyDetailsByStudentId(int studentId)
        {
            var familyDetails = await _studentFamilyDetailsService.GetStudentFamilyDetailsByStudentId(studentId);
            return Ok(familyDetails);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStudentFamilyDetails()
        {
            var familyDetailsList = await _studentFamilyDetailsService.GetAllStudentFamilyDetails();
            return Ok(familyDetailsList);
        }
    }
} 