using Core.Abstractions;
using Core.DTOs.App;
using Core.Entities;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Infrastructure
{
    public class ThemeActivityRepository(DatabaseContext context) : IThemeActivityRepository
    {
        private readonly DatabaseContext _context = context;

        public async Task<bool> SaveThemeActivity(ThemeActivity themeActivity)
        {
            if (themeActivity.Id > 0)
            {
                _context.ThemeActivities.Update(themeActivity);
            }
            else
            {
                _context.ThemeActivities.Add(themeActivity);
            }
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<ThemeActivity> GetThemeActivity(int id)
        {
            return await _context.ThemeActivities.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted) ?? new ThemeActivity();
        }

        public async Task<IEnumerable<ThemeActivityListDTO>> GetThemeActivityList(int? institutionId, int? themeId, int? gradeId, string section, DateTime? fromDate, DateTime? toDate, int createdBy)
        {
            using (var connection = _context.Database.GetDbConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@InstitutionId", institutionId);
                parameters.Add("@ThemeId", themeId);
                parameters.Add("@GradeId", gradeId);
                parameters.Add("@Section", section);
                parameters.Add("@FromDate", fromDate);
                parameters.Add("@ToDate", toDate);
                parameters.Add("@CreatedBy", createdBy);

                var result = await connection.QueryAsync<ThemeActivityListDTO>(
                    "usp_ThemeActivityList",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return result;
            }
        }

        public async Task<bool> DeleteThemeActivity(int id, int deletedBy)
        {
            var themeActivity = await _context.ThemeActivities.FirstOrDefaultAsync(x => x.Id == id);
            if (themeActivity != null)
            {
                themeActivity.IsDeleted = true;
                themeActivity.DeletedBy = deletedBy;
                themeActivity.DeletedDate = DateTime.UtcNow;
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }
    }
}