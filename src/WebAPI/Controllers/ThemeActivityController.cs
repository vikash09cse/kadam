using Core.Entities;
using Core.Features.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ThemeActivityController(ThemeActivityService themeActivityService) : ControllerBase
    {
        private readonly ThemeActivityService _themeActivityService = themeActivityService;

        [HttpPost]
        public async Task<IActionResult> SaveThemeActivity([FromBody] ThemeActivity themeActivity)
        {
            var response = await _themeActivityService.SaveThemeActivity(themeActivity);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetThemeActivity(int id)
        {
            var response = await _themeActivityService.GetThemeActivity(id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet]
        public async Task<IActionResult> GetThemeActivityList(
            [FromQuery] int? institutionId = null,
            [FromQuery] int? themeId = null,
            [FromQuery] int? gradeId = null,
            [FromQuery] string section = null,
            [FromQuery] int createdBy = 0)
        {
            var response = await _themeActivityService.GetThemeActivityList(institutionId, themeId, gradeId, section, createdBy);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{id}/{userId}")]
        public async Task<IActionResult> DeleteThemeActivity(int id, int userId)
        {
            var response = await _themeActivityService.DeleteThemeActivity(id, userId);
            return StatusCode(response.StatusCode, response);
        }
    }
}