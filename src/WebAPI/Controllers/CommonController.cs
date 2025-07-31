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
    public class CommonController(AdminService adminService) : ControllerBase
    {
        private readonly AdminService _adminService = adminService;

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
    }
} 