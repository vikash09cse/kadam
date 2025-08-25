using Core.DTOs.App;
using Core.Entities;

namespace Core.Abstractions
{
    public interface IStudentAttendanceRepository
    {
        Task<bool> SaveStudentAttendance(StudentAttendance studentAttendance);
        Task<StudentAttendance> GetStudentAttendance(int id);
        Task<IEnumerable<StudentListAttendanceDTO>> GetStudentAttendanceList(int? institutionId, int? gradeId, string section, DateTime? attendanceDate, int createdBy);
        Task<StudentAttendanceSaveResponseDTO> SaveStudentAttendanceBulk(List<StudentAttendanceSaveDTO> attendanceDataJson, int createdBy);
    }
}
