using Core.Features.Admin;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly AdminService _adminService;
        public LoginController(AdminService adminService)
        {
            _adminService = adminService;
        }

        public async Task<IActionResult> Get()
        {
            var response = await _adminService.GetProgram(1);
            return Ok(response);
        }

        //[HttpPost]
        //public IActionResult Login([FromBody] LoginRequest request)
        //{
        //    var user = _authenticationService.Authenticate(request.Username, request.Password);
        //    if (user == null)
        //    {
        //        return Unauthorized();
        //    }
        //    return Ok(user);
        //}
        //[HttpGet]
        //public IActionResult Logout()
        //{
        //    _authenticationService.Logout();
        //    return Ok();
        //}
    }
}
