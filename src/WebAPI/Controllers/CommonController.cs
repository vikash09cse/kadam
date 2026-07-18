using Core.DTOs;
using Core.Features.Admin;
using Core.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CommonController(AdminService adminService, IConfiguration configuration) : ControllerBase
    {
        private readonly AdminService _adminService = adminService;
        private readonly IConfiguration _configuration = configuration;

        /// <summary>
        /// Get current app version info for soft/force update checking.
        /// Anonymous — called by the mobile app on launch.
        /// Soft update: set LatestVersion higher than the installed app version (IsForceUpdate=false).
        /// Force update: set IsForceUpdate=true and MinimumRequiredVersion higher than the installed app version.
        /// Keep UpdateUrl pointed at the live APK download.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("app-version")]
        public IActionResult GetAppVersion()
        {
            try
            {
                var versionSettings = _configuration.GetSection("AppVersionSettings");
                var dto = new AppVersionDTO
                {
                    LatestVersion = versionSettings["LatestVersion"] ?? "1.0",
                    MinimumRequiredVersion = versionSettings["MinimumRequiredVersion"] ?? "1.0",
                    IsForceUpdate = bool.TryParse(versionSettings["IsForceUpdate"], out var forceUpdate) && forceUpdate,
                    UpdateUrl = versionSettings["UpdateUrl"] ?? string.Empty,
                    UpdateMessage = versionSettings["UpdateMessage"] ?? "A new version is available. Please update to continue."
                };
                var response = new ServiceResponseDTO(true, AppStatusCodes.Success, dto, "Version info retrieved.");
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                var response = new ServiceResponseDTO(false, AppStatusCodes.InternalServerError, null, $"Internal server error: {ex.Message}");
                return StatusCode(response.StatusCode, response);
            }
        }

        /// <summary>
        /// Get all states with active status
        /// </summary>
        /// <returns>ServiceResponseDTO containing list of states</returns>
        [HttpGet("states")]
        public async Task<IActionResult> GetStates()
        {
            try
            {
                var states = await _adminService.GetStatesByStatus(Enums.Status.Active);
                var response = new ServiceResponseDTO(true, AppStatusCodes.Success, states, MessageSuccess.RecordFound);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                var response = new ServiceResponseDTO(false, AppStatusCodes.InternalServerError, null, $"Internal server error: {ex.Message}");
                return StatusCode(response.StatusCode, response);
            }
        }

        /// <summary>
        /// Get districts by state ID
        /// </summary>
        /// <param name="stateId">State ID</param>
        /// <returns>ServiceResponseDTO containing list of districts for the specified state</returns>
        [HttpGet("districts/{stateId}")]
        public async Task<IActionResult> GetDistrictByStateId(int stateId)
        {
            try
            {
                var districts = await _adminService.GetDistrictsByState(stateId);
                var response = new ServiceResponseDTO(true, AppStatusCodes.Success, districts, MessageSuccess.RecordFound);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                var response = new ServiceResponseDTO(false, AppStatusCodes.InternalServerError, null, $"Internal server error: {ex.Message}");
                return StatusCode(response.StatusCode, response);
            }
        }

        /// <summary>
        /// Get all active themes
        /// </summary>
        /// <returns>ServiceResponseDTO containing list of active themes</returns>
        [HttpGet("themes")]
        public async Task<IActionResult> GetActiveThemes()
        {
            try
            {
                var themes = await _adminService.GetActiveThemes();
                var response = new ServiceResponseDTO(true, AppStatusCodes.Success, themes, MessageSuccess.RecordFound);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                var response = new ServiceResponseDTO(false, AppStatusCodes.InternalServerError, null, $"Internal server error: {ex.Message}");
                return StatusCode(response.StatusCode, response);
            }
        }
    }
} 