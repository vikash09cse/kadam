using Core.Abstractions;
using Core.DTOs;
using Core.DTOs.App;
using Core.Entities;
using Core.Utilities;
using static Core.Utilities.Enums;

namespace Core.Features.Admin
{
    public class StudentFollowupService
    {
        private readonly IStudentFollowupRepository _studentFollowupRepository;

        public StudentFollowupService(IStudentFollowupRepository studentFollowupRepository)
        {
            _studentFollowupRepository = studentFollowupRepository;
        }

        public async Task<ServiceResponseDTO> SaveStudentFollowup(StudentFollowup studentFollowup)
        {
            bool isSaved = await _studentFollowupRepository.SaveStudentFollowup(studentFollowup);
            return new ServiceResponseDTO(isSaved, isSaved ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, result: studentFollowup.Id, isSaved ? MessageSuccess.Saved : MessageError.CodeIssue);
        }

        public async Task<ServiceResponseDTO> GetStudentFollowup(int id)
        {
            var studentFollowup = await _studentFollowupRepository.GetStudentFollowup(id);
            ServiceResponseDTO response = new(true, AppStatusCodes.Success, studentFollowup, MessageSuccess.Found);
            return response;
        }

        public async Task<ServiceResponseDTO> GetStudentFollowupList(int? studentId, int? institutionId, int? gradeId, string section, DateTime? fromDate, DateTime? toDate, int createdBy)
        {
            var studentFollowupList = await _studentFollowupRepository.GetStudentFollowupList(studentId, institutionId, gradeId, section, fromDate, toDate, createdBy);
            ServiceResponseDTO response = new(true, AppStatusCodes.Success, studentFollowupList, MessageSuccess.Found);
            return response;
        }

        public async Task<ServiceResponseDTO> DeleteStudentFollowup(int id, int userId)
        {
            var isDeleted = await _studentFollowupRepository.DeleteStudentFollowup(id, userId);
            ServiceResponseDTO response = new(isDeleted, isDeleted ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isDeleted, isDeleted ? MessageSuccess.Deleted : MessageError.CodeIssue);
            return response;
        }
    }
}
