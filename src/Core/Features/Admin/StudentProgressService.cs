using Core.Abstractions;
using Core.DTOs;
using Core.Utilities;
using static Core.Utilities.DBConstant;
using Core.Entities;
using Core.DTOs.App;

namespace Core.Features.Admin
{
    public class StudentProgressService
    {
        private readonly IStudentProgressRepository _studentProgressRepository;

        public StudentProgressService(IStudentProgressRepository studentProgressRepository)
        {
            _studentProgressRepository = studentProgressRepository;
        }

        public async Task<ServiceResponseDTO> GetStudentProgressDetail(int studentId)
        {
            var studentProgress = await _studentProgressRepository.GetStudentProgressDetail(studentId);
            if (studentProgress != null)
            {
                var baselineDetails = await _studentProgressRepository.GetStudentBaselineDetailWithSubjects(studentId);
                if (baselineDetails.Any())
                {
                    studentProgress.Baselines = baselineDetails.Where(x => x.BaselineType == BaselineType.BaselinePreAssessment);
                    studentProgress.EndBaselines = baselineDetails.Where(x => x.BaselineType == BaselineType.EndlinePreAssessment);
                }
            }
            ServiceResponseDTO response = new(true, AppStatusCodes.Success, studentProgress, MessageSuccess.Found);
            return response;
        }
        public async Task<ServiceResponseDTO> SaveStudentProgress(StudentProgressStep studentProgress)
        {
            if (studentProgress == null)
            {
                return new ServiceResponseDTO(false, AppStatusCodes.BadRequest, null, MessageError.InvalidData);
            }

            var isSaved = await _studentProgressRepository.SaveStudentProgress(studentProgress);
            if (isSaved)
            {
                return new ServiceResponseDTO(true, AppStatusCodes.Success, isSaved, MessageSuccess.Saved);
            }
            else
            {
                return new ServiceResponseDTO(false, AppStatusCodes.InternalServerError, null, MessageError.CodeIssue);
            }
        }
        public async Task<ServiceResponseDTO> SaveStudentGradeTestDetail(StudentGradeTestDetailSaveDTO studentGradeTestDetail)
        {
            if (studentGradeTestDetail == null || studentGradeTestDetail.StudentGradeTestDetails == null || !studentGradeTestDetail.StudentGradeTestDetails.Any())
            {
                return new ServiceResponseDTO(false, AppStatusCodes.BadRequest, null, MessageError.InvalidData);
            }

            var isSaved = await _studentProgressRepository.SaveStudentGradeTestDetail(studentGradeTestDetail);
            if (isSaved)
            {
                return new ServiceResponseDTO(true, AppStatusCodes.Success, isSaved, MessageSuccess.Saved);
            }
            else
            {
                return new ServiceResponseDTO(false, AppStatusCodes.InternalServerError, null, MessageError.CodeIssue);
            }
        }
        public async Task<ServiceResponseDTO> GetStudentGradeTestDetailsWithSubjects(int studentId, int gradeLevelId)
        {
            var studentProgress = await _studentProgressRepository.GetStudentGradeTestDetailsWithSubjects(studentId, gradeLevelId);
            ServiceResponseDTO response = new(true, AppStatusCodes.Success, studentProgress, MessageSuccess.Found);
            return response;
        }
        public async Task<ServiceResponseDTO> CheckStudentPreviousGradeMarks(int studentId, int gradeLevelId)
        {
            if (studentId <= 0 || gradeLevelId <= 0)
            {
                return new ServiceResponseDTO(false, AppStatusCodes.BadRequest, null, MessageError.InvalidData);
            }

            var result = await _studentProgressRepository.CheckStudentPreviousGradeMarks(studentId, gradeLevelId);
            ServiceResponseDTO response = new(true, AppStatusCodes.Success, result, MessageSuccess.Found);
            return response;
        }
    }
}
