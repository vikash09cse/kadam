using Core.Entities;
using Core.Features.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Core.DTOs;
using Core.Utilities;

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

        [HttpPost("upload-certificate")]
        public async Task<IActionResult> UploadCertificate()
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
                string uploadsFolder = Path.Combine("UploadFiles", "HealthCertificates");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Delete previous certificate files for this student
                try
                {
                    // Find all files that start with the student ID
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
                    Console.WriteLine($"Error deleting previous certificates: {ex.Message}");
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
                string relativePath = $"/UploadFiles/HealthCertificates/{uniqueFileName}";

                // Call service to save the certificate path
                // var response = await _studentHealthService.SaveCertificatePath(studentId, relativePath);

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