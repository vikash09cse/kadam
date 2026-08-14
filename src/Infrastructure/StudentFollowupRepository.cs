using Core.Abstractions;
using Core.DTOs.App;
using Core.Entities;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Infrastructure
{
    public class StudentFollowupRepository(IDbSession db, DatabaseContext context) : IStudentFollowupRepository
    {
        private readonly IDbSession _db = db;
        private readonly DatabaseContext _context = context;

        public async Task<bool> SaveStudentFollowup(StudentFollowup studentFollowup)
        {
            studentFollowup.DateEntryPoint = 1;
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
            var parameters = new DynamicParameters();
            parameters.Add("@StudentId", studentId);
            parameters.Add("@InstitutionId", institutionId);
            parameters.Add("@GradeId", gradeId);
            parameters.Add("@Section", section);
            parameters.Add("@FromDate", fromDate);
            parameters.Add("@ToDate", toDate);
            parameters.Add("@CreatedBy", createdBy);

            return await _db.Connection.QueryAsync<StudentFollowupListDTO>(
                "usp_StudentFollowupList",
                parameters,
                _db.Transaction,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<StudentFollowupListDTO>> GetFollowupReport(int userId, StudentFollowupReportFilterDTO filter)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@StateId", filter.StateId);
            parameters.Add("@DivisionId", filter.DivisionId);
            parameters.Add("@InstitutionId", filter.InstitutionId);
            parameters.Add("@GradeId", filter.GradeId);
            parameters.Add("@Section", filter.Section);
            parameters.Add("@FromDate", filter.FromDate.Date);
            parameters.Add("@ToDate", filter.ToDate.Date);

            return await _db.Connection.QueryAsync<StudentFollowupListDTO>(
                "usp_StudentFollowupReport",
                parameters,
                _db.Transaction,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> DeleteStudentFollowup(int id, int deletedBy)
        {
            var studentFollowup = await _context.StudentFollowups.FirstOrDefaultAsync(x => x.Id == id);
            if (studentFollowup != null)
            {
                studentFollowup.IsDeleted = true;
                studentFollowup.DeletedBy = deletedBy;
                studentFollowup.DeletedDate = DateTime.UtcNow;
                studentFollowup.DateEntryPoint = 1;
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }
    }
}
