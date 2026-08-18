using Core.DTOs;
using Core.Features.Admin;
using Core.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Services
{
    public class PagePermissionGuard(
        PagePermissionService pagePermissionService,
        AuthenticationService authenticationService,
        IHttpContextAccessor httpContextAccessor)
    {
        public async Task<PageActionPermission> GetAsync()
        {
            var userId = authenticationService.GetCurrentUserId();
            var path = httpContextAccessor.HttpContext?.Request.Path.Value;
            return await pagePermissionService.GetAsync(userId, path);
        }

        public async Task<bool> CanAddEditAsync() => (await GetAsync()).CanAddEdit;

        public async Task<bool> CanDeleteAsync() => (await GetAsync()).CanDelete;

        public async Task<IActionResult?> ForbidAddEditAsync()
        {
            return (await CanAddEditAsync()) ? null : Denied();
        }

        public async Task<IActionResult?> ForbidDeleteAsync()
        {
            return (await CanDeleteAsync()) ? null : Denied();
        }

        public static IActionResult Denied()
        {
            return new JsonResult(new { success = false, message = MessageError.NoPermission });
        }
    }
}
