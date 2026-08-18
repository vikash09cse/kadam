using Core.DTOs.App;

namespace Core.Abstractions
{
    public interface IThemeActivityRepository
    {
        Task<int> SaveThemeActivity(ThemeActivitySaveDTO themeActivity);
        Task<ThemeActivityDetailDTO> GetThemeActivity(int id);
        Task<IEnumerable<ThemeActivityListDTO>> GetThemeActivityList(int? institutionId, int? themeId, int? gradeId, string section, DateTime? fromDate, DateTime? toDate, int createdBy);
        Task<IEnumerable<ThemeActivityReportDTO>> GetThemeActivityReport(int userId, ThemeActivityReportFilterDTO filter);
        Task<bool> DeleteThemeActivity(int id, int deletedBy);
        Task<IEnumerable<AppInstitutionThemeActivityDTO>> GetInstitutionsByUserIdForThemeActivity(int userId);
    }
}