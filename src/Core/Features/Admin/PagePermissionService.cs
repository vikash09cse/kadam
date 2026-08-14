using Core.Abstractions;
using Core.DTOs;
using Core.Utilities;

namespace Core.Features.Admin
{
    public class PagePermissionService(IAdminRepository adminRepository)
    {
        public async Task<PageActionPermission> GetAsync(int userId, string? requestPath)
        {
            if (userId <= 0)
            {
                return PageActionPermission.None;
            }

            var menuUrl = MenuActionRules.NormalizeMenuUrl(requestPath);
            if (string.IsNullOrWhiteSpace(menuUrl))
            {
                return PageActionPermission.None;
            }

            return await adminRepository.GetPageActionPermission(userId, menuUrl);
        }

        public async Task<bool> CanAddEditAsync(int userId, string? requestPath)
        {
            return (await GetAsync(userId, requestPath)).CanAddEdit;
        }

        public async Task<bool> CanDeleteAsync(int userId, string? requestPath)
        {
            return (await GetAsync(userId, requestPath)).CanDelete;
        }
    }
}
