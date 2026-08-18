using Core.Abstractions;
using Core.DTOs.App;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Data;

namespace Infrastructure
{
    public class ThemeActivityRepository(IDbSession db, DatabaseContext context) : IThemeActivityRepository
    {
        private readonly IDbSession _db = db;
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

        public async Task<IEnumerable<ThemeActivityReportDTO>> GetThemeActivityReport(int userId, ThemeActivityReportFilterDTO filter)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@StateId", filter.StateId);
            parameters.Add("@DivisionId", filter.DivisionId);
            parameters.Add("@InstitutionId", filter.InstitutionId);
            parameters.Add("@ThemeId", filter.ThemeId);
            parameters.Add("@GradeId", filter.GradeId);
            parameters.Add("@Section", filter.Section);
            parameters.Add("@FromDate", filter.FromDate.Date);
            parameters.Add("@ToDate", filter.ToDate.Date);

            return await _db.Connection.QueryAsync<ThemeActivityReportDTO>(
                "usp_ThemeActivityReport",
                parameters,
                _db.Transaction,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> DeleteThemeActivity(int id, int deletedBy)
        {
            var themeActivity = await _context.ThemeActivities.FirstOrDefaultAsync(x => x.Id == id);
            if (themeActivity != null)
            {
                themeActivity.IsDeleted = true;
                themeActivity.DeletedBy = deletedBy;
                themeActivity.DeletedDate = DateTime.UtcNow;
                themeActivity.DateEntryPoint = 1;
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