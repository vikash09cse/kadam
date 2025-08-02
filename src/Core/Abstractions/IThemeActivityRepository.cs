using Core.DTOs.App;
using Core.Entities;

namespace Core.Abstractions
{
    public interface IThemeActivityRepository
    {
        Task<bool> SaveThemeActivity(ThemeActivity themeActivity);
        Task<ThemeActivity> GetThemeActivity(int id);
        Task<IEnumerable<ThemeActivityListDTO>> GetThemeActivityList(int? institutionId, int? themeId, int? gradeId, string section, int createdBy);
        Task<bool> DeleteThemeActivity(int id, int deletedBy);
    }
}