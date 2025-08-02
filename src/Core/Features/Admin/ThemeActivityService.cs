using Core.Abstractions;
using Core.DTOs;
using Core.DTOs.App;
using Core.Entities;
using Core.Utilities;
using static Core.Utilities.Enums;

namespace Core.Features.Admin
{
    public class ThemeActivityService
    {
        private readonly IThemeActivityRepository _themeActivityRepository;

        public ThemeActivityService(IThemeActivityRepository themeActivityRepository)
        {
            _themeActivityRepository = themeActivityRepository;
        }

        public async Task<ServiceResponseDTO> SaveThemeActivity(ThemeActivity themeActivity)
        {
            bool isSaved = await _themeActivityRepository.SaveThemeActivity(themeActivity);
            return new ServiceResponseDTO(isSaved, isSaved ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, result: themeActivity.Id, isSaved ? MessageSuccess.Saved : MessageError.CodeIssue);
        }

        public async Task<ServiceResponseDTO> GetThemeActivity(int id)
        {
            var themeActivity = await _themeActivityRepository.GetThemeActivity(id);
            ServiceResponseDTO response = new(true, AppStatusCodes.Success, themeActivity, MessageSuccess.Found);
            return response;
        }

        public async Task<ServiceResponseDTO> GetThemeActivityList(int? institutionId, int? themeId, int? gradeId, string section, int createdBy)
        {
            var themeActivityList = await _themeActivityRepository.GetThemeActivityList(institutionId, themeId, gradeId, section, createdBy);
            ServiceResponseDTO response = new(true, AppStatusCodes.Success, themeActivityList, MessageSuccess.Found);
            return response;
        }

        public async Task<ServiceResponseDTO> DeleteThemeActivity(int id, int userId)
        {
            var isDeleted = await _themeActivityRepository.DeleteThemeActivity(id, userId);
            ServiceResponseDTO response = new(isDeleted, isDeleted ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isDeleted, isDeleted ? MessageSuccess.Deleted : MessageError.CodeIssue);
            return response;
        }
    }
}