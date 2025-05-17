using Core.Abstractions;
using Core.DTOs;
using Core.DTOs.App;
using Core.Entities;
using Core.Utilities;
using static Core.Utilities.Enums;

namespace Core.Features.Admin
{
    public class StudentBaselineDetailService
    {
        private readonly IStudentBaselineDetailRepository _repository;

        public StudentBaselineDetailService(IStudentBaselineDetailRepository repository)
        {
            _repository = repository;
        }

        public async Task<ServiceResponseDTO> GetAllAsync()
        {
            var result = await _repository.GetAllStudentBaselineDetails();
            return new ServiceResponseDTO(true, AppStatusCodes.Success, result, MessageSuccess.Found);
        }

        public async Task<ServiceResponseDTO> GetByIdAsync(int id)
        {
            var result = await _repository.GetStudentBaselineDetail(id);
            return new ServiceResponseDTO(true, AppStatusCodes.Success, result, MessageSuccess.Found);
        }

        public async Task<ServiceResponseDTO> GetByStudentIdAsync(int studentId)
        {
            if (studentId <= 0)
                return new ServiceResponseDTO(false, AppStatusCodes.BadRequest, true, MessageError.InvalidStudentId);

            var result = await _repository.GetStudentBaselineDetailsByStudentId(studentId);
            return new ServiceResponseDTO(true, AppStatusCodes.Success, result, MessageSuccess.Found);
        }

        public async Task<ServiceResponseDTO> GetStudentBaselineDetailWithSubjects(int studentId, string baselineType)
        {
            if (studentId <= 0)
                return new ServiceResponseDTO(false, AppStatusCodes.BadRequest, true, MessageError.InvalidStudentId);

            var result = await _repository.GetStudentBaselineDetailWithSubjects(studentId, baselineType);
            return new ServiceResponseDTO(true, AppStatusCodes.Success, result, MessageSuccess.Found);
        }

        public async Task<ServiceResponseDTO> SaveStudentBaselineDetail(StudentBaselineDetail entity)
        {
            if (entity == null)
                return new ServiceResponseDTO(false, AppStatusCodes.BadRequest, true, MessageError.InvalidData);

            // Validate required fields
            if (entity.StudentId <= 0)
                return new ServiceResponseDTO(false, AppStatusCodes.BadRequest, true, MessageError.StudentIdRequired);

            if (entity.SubjectId <= 0)
                return new ServiceResponseDTO(false, AppStatusCodes.BadRequest, true, MessageError.SubjectIdRequired);

            var isSaved = await _repository.SaveStudentBaselineDetail(entity);
            return new ServiceResponseDTO(isSaved, isSaved ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, 
                result: entity.Id, isSaved ? MessageSuccess.Saved : MessageError.CodeIssue);
        }

        public async Task<ServiceResponseDTO> DeleteAsync(int id, int deletedBy)
        {
            var isDeleted = await _repository.DeleteStudentBaselineDetail(id, deletedBy);
            return new ServiceResponseDTO(isDeleted, isDeleted ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, 
                isDeleted, isDeleted ? MessageSuccess.Deleted : MessageError.CodeIssue);
        }

        public async Task<ServiceResponseDTO> SaveStudentBaselineDetail(StudentBaselineDetailWithSubjectSaveDTO studentBaselineDetail)
        {
            var isSaved = await _repository.SaveStudentBaselineDetail(studentBaselineDetail);
            return new ServiceResponseDTO(isSaved, isSaved ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, 
                result: isSaved, isSaved ? MessageSuccess.Saved : MessageError.CodeIssue);
        }
    }
} 