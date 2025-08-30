using Core.Entities;
using Core.Features.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentTrioController(StudentTrioService studentTrioService) : ControllerBase
    {
        private readonly StudentTrioService _studentTrioService = studentTrioService;

        [HttpPost]
        public async Task<IActionResult> SaveStudentTrio([FromBody] StudentTrio studentTrio)
        {
            var response = await _studentTrioService.SaveStudentTrio(studentTrio);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet]
        public async Task<IActionResult> GetStudentTrios()
        {
            var response = await _studentTrioService.GetStudentTrios();
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudentTrio(int id)
        {
            var response = await _studentTrioService.GetStudentTrio(id);
            return StatusCode(response.StatusCode, response);
        }
    }
}
