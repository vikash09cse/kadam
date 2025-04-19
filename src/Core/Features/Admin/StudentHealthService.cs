using Core.Abstractions;
using Core.DTOs;
using Core.DTOs.App;
using Core.Entities;
using Core.Utilities;
using static Core.Utilities.Enums;

namespace Core.Features.Admin
{
    public class StudentHealthService
    {
        private readonly IStudentHealthRepository _studentHealthRepository;

        public StudentHealthService(IStudentHealthRepository studentHealthRepository)
        {
            _studentHealthRepository = studentHealthRepository;
        }

        public async Task<ServiceResponseDTO> SaveStudentHealth(StudentHealth health)
        {
            bool isSaved = await _studentHealthRepository.SaveStudentHealth(health);
            return new ServiceResponseDTO(isSaved, isSaved ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, 
                result: health.Id, isSaved ? MessageSuccess.Saved : MessageError.CodeIssue);
        }

        public async Task<ServiceResponseDTO> DeleteStudentHealth(int id, int userId)
        {
            var isDeleted = await _studentHealthRepository.DeleteStudentHealth(id, userId);
            return new ServiceResponseDTO(isDeleted, isDeleted ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, 
                isDeleted, isDeleted ? MessageSuccess.Deleted : MessageError.CodeIssue);
        }

        public async Task<ServiceResponseDTO> GetStudentHealth(int id)
        {
            var health = await _studentHealthRepository.GetStudentHealth(id);
            return new ServiceResponseDTO(true, AppStatusCodes.Success, health, MessageSuccess.Found);
        }

        public async Task<ServiceResponseDTO> GetStudentHealthByStudentId(int studentId)
        {
            var health = await _studentHealthRepository.GetStudentHealthByStudentId(studentId);
            return new ServiceResponseDTO(true, AppStatusCodes.Success, health, MessageSuccess.Found);
        }

        public async Task<ServiceResponseDTO> GetAllStudentHealths()
        {
            var healthList = await _studentHealthRepository.GetAllStudentHealths();
            return new ServiceResponseDTO(true, AppStatusCodes.Success, healthList, MessageSuccess.Found);
        }

        public async Task<ServiceResponseDTO> GetHealthDefaultData(int studentid)
        {
            var studentDefaultData = new StudentHealthRoot
            {
                PhysicalChallengedTypes = EnumHelper<PhysicalChallengedTypes>.GetEnumDropdownList(),
                HealthInfo = new StudentHealth()
            };
            if (studentid != 0)
            {
                var health = await _studentHealthRepository.GetStudentHealthByStudentId(studentid);
                if (health != null)
                {
                    studentDefaultData.HealthInfo = health;
                }
            }

            ServiceResponseDTO response = new(true, AppStatusCodes.Success, studentDefaultData, MessageSuccess.Found);
            return response;
        }

        public async Task<ServiceResponseDTO> SaveCertificatePath(int studentId, string relativePath)
        {
            try
            {
                // Get health record and update certificate path
                var health = await _studentHealthRepository.GetStudentHealthByStudentId(studentId);
                if (health != null && health.Id > 0)
                {
                    // Update the certificate path
                    health.DisabilityCertificatePath = relativePath;
                    
                    // Save the updated record
                    bool isSaved = await _studentHealthRepository.SaveStudentHealth(health);
                    return new ServiceResponseDTO(isSaved, 
                        isSaved ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, 
                        relativePath, 
                        isSaved ? MessageSuccess.Saved : MessageError.CodeIssue);
                }
                else
                {
                    // Create new health record if it doesn't exist
                    var newHealth = new StudentHealth
                    {
                        StudentId = studentId,
                        DisabilityCertificatePath = relativePath,
                        PhysicallyChallenged = true,
                        CurrentStatus = Status.Active
                    };
                    
                    bool isSaved = await _studentHealthRepository.SaveStudentHealth(newHealth);
                    return new ServiceResponseDTO(isSaved, 
                        isSaved ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, 
                        relativePath, 
                        isSaved ? MessageSuccess.Saved : MessageError.CodeIssue);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO(false, 400, null, $"Error saving certificate path: {ex.Message}");
            }
        }
    }
} 