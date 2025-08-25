using Core.Abstractions;
using Core.DTOs.App;
using Core.Entities;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Data;

namespace Infrastructure
{
    public class StudentAttendanceRepository(DatabaseContext context) : IStudentAttendanceRepository
    {
        private readonly DatabaseContext _context = context;

        public async Task<bool> SaveStudentAttendance(StudentAttendance studentAttendance)
        {
            if (studentAttendance.Id > 0)
            {
                _context.StudentAttendances.Update(studentAttendance);
            }
            else
            {
                _context.StudentAttendances.Add(studentAttendance);
            }
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<StudentAttendance> GetStudentAttendance(int id)
        {
            return await _context.StudentAttendances.FirstOrDefaultAsync(x => x.Id == id) ?? new StudentAttendance();
        }

        public async Task<IEnumerable<StudentListAttendanceDTO>> GetStudentAttendanceList(int? institutionId, int? gradeId, string section, DateTime? attendanceDate, int createdBy)
        {
            using (var connection = _context.Database.GetDbConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@InstitutionId", institutionId);
                parameters.Add("@GradeId", gradeId);
                parameters.Add("@Section", section);
                parameters.Add("@AttendanceDate", attendanceDate);
                parameters.Add("@CreatedBy", createdBy);

                var result = await connection.QueryAsync<StudentListAttendanceDTO>(
                    "usp_StudentList_Attendance_Mobile",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return result;
            }
        }

        public async Task<StudentAttendanceSaveResponseDTO> SaveStudentAttendanceBulk(List<StudentAttendanceSaveDTO> attendanceDataJson, int createdBy)
        {
            //var json = 
            var json = JsonConvert.SerializeObject(attendanceDataJson);

            using (var connection = _context.Database.GetDbConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@AttendanceData", json);
                parameters.Add("@CreatedBy", createdBy);

                var result = await connection.QuerySingleAsync<StudentAttendanceSaveResponseDTO>(
                    "usp_StudentAttendanceSave",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return result;
            }
        }
    }
}
