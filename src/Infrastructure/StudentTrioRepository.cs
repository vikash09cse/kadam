using Core.Abstractions;
using Core.Entities;
using Core.DTOs;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Infrastructure
{
    public class StudentTrioRepository(IDbSession db, DatabaseContext context) : IStudentTrioRepository
    {
        private readonly IDbSession _db = db;
        private readonly DatabaseContext _context = context;

        public async Task<bool> SaveStudentTrio(StudentTrio studentTrio)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@StudentId", studentTrio.StudentId);
            parameters.Add("@TrioId", studentTrio.TrioId);
            parameters.Add("@CreatedBy", studentTrio.CreatedBy);

            try
            {
                var result = await _db.Connection.ExecuteAsync("usp_SaveStudentTrio", parameters, commandType: CommandType.StoredProcedure);
                return result > 0;
            }
            catch (Exception ex)
            {
                // Log the error if needed
                return false;
            }
        }

        public async Task<IEnumerable<StudentTrio>> GetStudentTrios()
        {
            return await _context.StudentTrios
                .Where(x => !x.IsDeleted)
                .ToListAsync();
        }

        public async Task<StudentTrio> GetStudentTrio(int id)
        {
            return await _context.StudentTrios
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted) ?? new StudentTrio();
        }

        public async Task<TrioCapacityCheckDTO> CheckTrioCapacity(int studentId, int trioId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@StudentId", studentId);
            parameters.Add("@TrioId", trioId);

            try
            {
                var result = await _db.Connection.QuerySingleAsync<TrioCapacityCheckDTO>(
                    "usp_CheckTrioCapacity", 
                    parameters, 
                    commandType: CommandType.StoredProcedure);

                return result;
            }
            catch (Exception ex)
            {
                // Return default response on error
                return new TrioCapacityCheckDTO
                {
                    HasCapacity = false,
                    Message = "Error checking trio capacity"
                };
            }
        }
    }
}
