using Core.Abstractions;
using Core.DTOs.App;
using Core.Entities;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Infrastructure
{
    public class StudentFollowupRepository(DatabaseContext context) : IStudentFollowupRepository
    {
        private readonly DatabaseContext _context = context;

        public async Task<bool> SaveStudentFollowup(StudentFollowup studentFollowup)
        {
            if (studentFollowup.Id > 0)
            {
                _context.StudentFollowups.Update(studentFollowup);
            }
            else
            {
                _context.StudentFollowups.Add(studentFollowup);
            }
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<StudentFollowup> GetStudentFollowup(int id)
        {
            return await _context.StudentFollowups.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted) ?? new StudentFollowup();
        }

        public async Task<IEnumerable<StudentFollowupListDTO>> GetStudentFollowupList(int? studentId, int? institutionId, int? gradeId, string section, DateTime? fromDate, DateTime? toDate, int createdBy)
        {
            using (var connection = _context.Database.GetDbConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@StudentId", studentId);
                parameters.Add("@InstitutionId", institutionId);
                parameters.Add("@GradeId", gradeId);
                parameters.Add("@Section", section);
                parameters.Add("@FromDate", fromDate);
                parameters.Add("@ToDate", toDate);
                parameters.Add("@CreatedBy", createdBy);

                var result = await connection.QueryAsync<StudentFollowupListDTO>(
                    "usp_StudentFollowupList",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return result;
            }
        }

        public async Task<bool> DeleteStudentFollowup(int id, int deletedBy)
        {
            var studentFollowup = await _context.StudentFollowups.FirstOrDefaultAsync(x => x.Id == id);
            if (studentFollowup != null)
            {
                studentFollowup.IsDeleted = true;
                studentFollowup.DeletedBy = deletedBy;
                studentFollowup.DeletedDate = DateTime.UtcNow;
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }
    }
}
