using Core.Abstractions;
using Core.DTOs;
using Core.Entities;
using Core.Utilities;
using static Core.Utilities.Enums;

namespace Core.Features.Admin
{
    public class StudentFamilyDetailsService
    {
        private readonly IStudentFamilyDetailsRepository _studentFamilyDetailsRepository;

        public StudentFamilyDetailsService(IStudentFamilyDetailsRepository studentFamilyDetailsRepository)
        {
            _studentFamilyDetailsRepository = studentFamilyDetailsRepository;
        }

        public async Task<ServiceResponseDTO> SaveStudentFamilyDetails(StudentFamilyDetail familyDetails)
        {
            bool isSaved = await _studentFamilyDetailsRepository.SaveStudentFamilyDetails(familyDetails);
            return new ServiceResponseDTO(isSaved, isSaved ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, 
                result: familyDetails.Id, isSaved ? MessageSuccess.Saved : MessageError.CodeIssue);
        }

        public async Task<ServiceResponseDTO> DeleteStudentFamilyDetails(int id, int userId)
        {
            var isDeleted = await _studentFamilyDetailsRepository.DeleteStudentFamilyDetails(id, userId);
            return new ServiceResponseDTO(isDeleted, isDeleted ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, 
                isDeleted, isDeleted ? MessageSuccess.Deleted : MessageError.CodeIssue);
        }

        public async Task<StudentFamilyDetail> GetStudentFamilyDetails(int id)
        {
            return await _studentFamilyDetailsRepository.GetStudentFamilyDetails(id);
        }

        public async Task<ServiceResponseDTO> GetStudentFamilyDetailsByStudentId(int studentId)
        {
            var studentFamilyInfo = await _studentFamilyDetailsRepository.GetStudentFamilyDetailsByStudentId(studentId);
            return new(true, AppStatusCodes.Success, studentFamilyInfo, MessageSuccess.Found);
        }
        public async Task<IEnumerable<StudentFamilyDetail>> GetAllStudentFamilyDetails()
        {
            return await _studentFamilyDetailsRepository.GetAllStudentFamilyDetails();
        }
    }
} 