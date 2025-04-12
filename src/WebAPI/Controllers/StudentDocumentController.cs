using Core.Entities;
using Core.Features.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentDocumentController(StudentDocumentService studentDocumentService) : ControllerBase
    {
        private readonly StudentDocumentService _studentDocumentService = studentDocumentService;

        [HttpPost]
        public async Task<IActionResult> SaveStudentDocument([FromBody] StudentDocument document)
        {
            var response = await _studentDocumentService.SaveStudentDocument(document);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{id}/{userId}")]
        public async Task<IActionResult> DeleteStudentDocument(int id, int userId)
        {
            var response = await _studentDocumentService.DeleteStudentDocument(id, userId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudentDocument(int id)
        {
            var response = await _studentDocumentService.GetStudentDocument(id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetStudentDocumentsByStudentId(int studentId)
        {
            var response = await _studentDocumentService.GetStudentDocumentsByStudentId(studentId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStudentDocuments()
        {
            var response = await _studentDocumentService.GetAllStudentDocuments();
            return StatusCode(response.StatusCode, response);
        }
    }
} 