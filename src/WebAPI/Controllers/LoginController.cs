using Core.DTOs;
using Core.DTOs.App;
using Core.DTOs.Users;
using Core.Features.Admin;
using Microsoft.AspNetCore.Mvc;
using WebAPI.AuthServices;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class LoginController(AdminService adminService, AuthenticationService authenticationService) : ControllerBase
    {
        private readonly AdminService _adminService = adminService;
        private readonly AuthenticationService _authenticationService = authenticationService;


        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Login Controller");
        }

        [HttpPost]
        [Route("LoginToken")]
        public async Task<IActionResult> LoginToken(LoginRequest request)
        {
            var userResponse = await _adminService.ValidateUser(request.Email, request.Password);
            if (userResponse is null || !userResponse.Success)
            {
                return Ok(userResponse);
            }

            if (userResponse.Result is not UserLoginValidateDTO userLoginInfo)
            {
                return Unauthorized();
            }

            // Create a new anonymous object excluding password fields
            var userDto = new
            {
                userLoginInfo.Id,
                userLoginInfo.RoleId,
                userLoginInfo.FirstName,
                userLoginInfo.LastName,
                userLoginInfo.Email,
                userLoginInfo.ReporteeRoleId,
                userLoginInfo.UserName,
                Token = _authenticationService.GenerateSecureToken(userLoginInfo)
            };

            var response = new ServiceResponseDTO(true, AppStatusCodes.Success, userDto, "");
            return Ok(response);
        }
    }
}
