using Core.Abstractions;
using Core.DTOs;
using Core.DTOs.App;
using Core.Entities;
using Core.Utilities;

namespace Core.Features.Admin
{
    public class StudentAttendanceService
    {
        private readonly IStudentAttendanceRepository _studentAttendanceRepository;

        public StudentAttendanceService(IStudentAttendanceRepository studentAttendanceRepository)
        {
            _studentAttendanceRepository = studentAttendanceRepository;
        }

        public async Task<ServiceResponseDTO> SaveStudentAttendance(StudentAttendance studentAttendance)
        {
            bool isSaved = await _studentAttendanceRepository.SaveStudentAttendance(studentAttendance);
            return new ServiceResponseDTO(isSaved, isSaved ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, result: studentAttendance.Id, isSaved ? MessageSuccess.Saved : MessageError.CodeIssue);
        }

        public async Task<ServiceResponseDTO> GetStudentAttendance(int id)
        {
            var studentAttendance = await _studentAttendanceRepository.GetStudentAttendance(id);
            ServiceResponseDTO response = new(true, AppStatusCodes.Success, studentAttendance, MessageSuccess.Found);
            return response;
        }

        public async Task<ServiceResponseDTO> GetStudentAttendanceList(int? institutionId, int? gradeId, string section, DateTime? attendanceDate, int createdBy)
        {
            var studentAttendanceList = await _studentAttendanceRepository.GetStudentAttendanceList(institutionId, gradeId, section, attendanceDate, createdBy);
            ServiceResponseDTO response = new(true, AppStatusCodes.Success, studentAttendanceList, MessageSuccess.Found);
            return response;
        }

        public async Task<ServiceResponseDTO> SaveStudentAttendanceBulk(List<StudentAttendanceSaveDTO> attendanceDataJson, int createdBy)
        {
            var result = await _studentAttendanceRepository.SaveStudentAttendanceBulk(attendanceDataJson, createdBy);
            
            if (result.Status == "Success")
            {
                return new ServiceResponseDTO(true, AppStatusCodes.Success, result, MessageSuccess.Saved);
            }
            else
            {
                return new ServiceResponseDTO(false, AppStatusCodes.BadRequest, result, result.ErrorMessage);
            }
        }
    }
}
