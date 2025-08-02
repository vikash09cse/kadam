using Core.DTOs;
using Core.DTOs.App;
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

        //[AllowAnonymous]
        [HttpGet("GetStudentListMyInstitution")]
        public async Task<IActionResult> GetStudentListMyInstitutionMobile(
            [FromQuery] int? institutionId,
            [FromQuery] int? gradeId = null,
            [FromQuery] string section = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int? currentStatus = null,
            [FromQuery] int createdBy = 0)
        {
            var students = await _StudentService.GetStudentListMyInstitutionMobile(institutionId, gradeId, section, fromDate, toDate, currentStatus, createdBy);
            return Ok(students);
        }

        [HttpGet("defaultdata/{userId}")]
        public async Task<IActionResult> GetStudentDefaultData(int userId)
        {
            var studentDefaultData = await _StudentService.GetStudentDefaultData(userId);
            return Ok(studentDefaultData);
        }

        [HttpGet("institutiongrade/{studentId}")]
        public async Task<IActionResult> GetInstitutionGradeByStudentId(int studentId)
        {
            var response = await _StudentService.GetInstitutionGradeByStudentId(studentId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("upload-profile-picture")]
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
                string uploadsFolder = Path.Combine("UploadFiles", "StudentProfilePicture");
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
                string relativePath = $"/UploadFiles/StudentProfilePicture/{uniqueFileName}";

                var isSaved = await _StudentService.SaveStudentProfilePicture(studentId, relativePath);
                if (!isSaved.Success)
                {
                    var _response = new ServiceResponseDTO(false, AppStatusCodes.InternalServerError, relativePath, MessageError.FailedToSaveProfilePicture);
                    return StatusCode(_response.StatusCode, _response);
                }

                var response = new ServiceResponseDTO(true, AppStatusCodes.Success, relativePath, MessageSuccess.Saved);

                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                return StatusCode(AppStatusCodes.InternalServerError, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPost("update-student-promotion")]
        public async Task<IActionResult> UpdateStudentPromotion([FromBody] StudentPromotionUpdateDTO studentPromotionUpdateDTO)
        {
            var response = await _StudentService.UpdateStudentPromotion(studentPromotionUpdateDTO);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("dashboard-count/{createdBy}")]
        public async Task<IActionResult> GetDashboardCount(int createdBy)
        {
            var response = await _StudentService.GetDashboardCount(createdBy);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("update-student-status")]
        public async Task<IActionResult> UpdateStudentStatus([FromBody] StudentStatusUpdateDTO studentStatusUpdateDTO)
        {
            var response = await _StudentService.UpdateStudentStatus(studentStatusUpdateDTO);
            return StatusCode(response.StatusCode, response);
        }
        [AllowAnonymous]
        [HttpGet("mainstream-detail/{id}")]
        public async Task<IActionResult> GetStudentDetailForMainstream(int id)
        {
            var response = await _StudentService.GetStudentDetailForMainstream(id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("save-student-mainstream")]
        public async Task<IActionResult> SaveStudentMainstream([FromBody] StudentMainstream studentMainstream)
        {
            var response = await _StudentService.SaveStudentMainstream(studentMainstream);
            return StatusCode(response.StatusCode, response);
        }
    }
}
