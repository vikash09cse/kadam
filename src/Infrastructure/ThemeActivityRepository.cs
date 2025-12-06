using Core.Abstractions;
using Core.DTOs.App;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Data;

namespace Infrastructure
{
    public class ThemeActivityRepository(DatabaseContext context) : IThemeActivityRepository
    {
        private readonly DatabaseContext _context = context;

        public async Task<int> SaveThemeActivity(ThemeActivitySaveDTO themeActivity)
        {
            using (var connection = _context.Database.GetDbConnection())
            {
                var gradeSectionsJson = JsonConvert.SerializeObject(themeActivity.GradeSections);

                var parameters = new DynamicParameters();
                parameters.Add("@Id", themeActivity.Id);
                parameters.Add("@ThemeId", themeActivity.ThemeId);
                parameters.Add("@InstitutionId", themeActivity.InstitutionId);
                parameters.Add("@TotalStudents", themeActivity.TotalStudents);
                parameters.Add("@StudentAttended", themeActivity.StudentAttended);
                parameters.Add("@DidChildrenDayHappen", themeActivity.DidChildrenDayHappen);
                parameters.Add("@TotalParentsAttended", themeActivity.TotalParentsAttended);
                parameters.Add("@ThemeActivityDate", themeActivity.ThemeActivityDate);
                parameters.Add("@CreatedBy", themeActivity.CreatedBy);
                parameters.Add("@GradeSections", gradeSectionsJson);

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "usp_SaveThemeActivity",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                if (result != null && result.Success == 1)
                {
                    return result.Id;
                }
                return 0;
            }
        }

        public async Task<ThemeActivityDetailDTO> GetThemeActivity(int id)
        {
            using (var connection = _context.Database.GetDbConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Id", id);

                var result = await connection.QueryMultipleAsync(
                    "usp_GetThemeActivity",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                var themeActivity = await result.ReadFirstOrDefaultAsync<ThemeActivityDetailDTO>();
                var gradeSections = await result.ReadAsync<ThemeActivityGradeSectionDTO>();

                if (themeActivity != null)
                {
                    themeActivity.GradeSections = gradeSections.ToList();
                }

                return themeActivity ?? new ThemeActivityDetailDTO();
            }
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

        public async Task<IEnumerable<AppInstitutionThemeActivityDTO>> GetInstitutionsByUserIdForThemeActivity(int userId)
        {
            using (var connection = _context.Database.GetDbConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);

                using var multi = await connection.QueryMultipleAsync("usp_GetInstitutionByUserIdForThemeActivity", parameters, commandType: CommandType.StoredProcedure);

                var institutions = await multi.ReadAsync<AppInstitutionThemeActivityDTO>();
                var grades = await multi.ReadAsync<AppGradeSectionThemeActivityDTO>();

                var result = institutions.ToList();
                foreach (var institution in result)
                {
                    institution.GradeSections = grades.Where(x => x.InstitutionId == institution.Id).ToList();
                }

                return result;
            }
        }
    }
}