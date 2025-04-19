using Core.DTOs;
using Core.Entities;
using Core.Features.Admin;
using Core.Utilities;
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

        [HttpPost("SaveStudentDocument")]
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

        [HttpGet("default-data/{studentId}")]
        public async Task<IActionResult> GetDocumentDefaultData(int studentId)
        {
            var response = await _studentDocumentService.GetDocumentDefaultData(studentId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost("upload-document")]
        public async Task<IActionResult> UploadDocument()
        {
            try
            {
                // Extract studentId from form data
                if (!Request.Form.TryGetValue("studentId", out var studentIdValues) ||
                    !int.TryParse(studentIdValues.FirstOrDefault(), out int studentId))
                {
                    return BadRequest("Student ID is required.");
                }

                // Get the file from the form data
                var file = Request.Form.Files.FirstOrDefault();
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded.");
                }

                // Ensure directory exists
                string uploadsFolder = Path.Combine("UploadFiles", "Documents");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Delete previous documents of the same type for this student
                try
                {
                    // Find all files that match the pattern for this student and document type
                    string searchPattern = $"{studentId}_*";
                    string[] existingFiles = Directory.GetFiles(uploadsFolder, searchPattern);
                    
                    // Delete each existing file
                    foreach (string existingFilePath in existingFiles)
                    {
                        System.IO.File.Delete(existingFilePath);
                    }
                }
                catch (Exception ex)
                {
                    // Log the error but continue with upload
                    Console.WriteLine($"Error deleting previous documents: {ex.Message}");
                }

                // Create unique filename with original extension
                string fileExtension = Path.GetExtension(file.FileName);
                string uniqueFileName = $"{studentId}_{Guid.NewGuid()}{fileExtension}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Get relative path to store in database
                string relativePath = $"/UploadFiles/Documents/{uniqueFileName}";

                var response = new ServiceResponseDTO(true, AppStatusCodes.Success, relativePath, MessageSuccess.Saved);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }
    }
} 