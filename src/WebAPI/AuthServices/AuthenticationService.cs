using Core.DTOs.Users;
using Core.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WebAPI.AuthServices
{
    public class AuthenticationService
    {
        private readonly string _jwtSecretKey;

        public AuthenticationService(IConfiguration configuration)
        {
            _jwtSecretKey = AppSettings.JWTSettings?.JwtSecretKey ?? throw new InvalidOperationException("JWT secret key is not found in the configuration file.");
        }

        public string GenerateSecureToken(UserLoginValidateDTO userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            //    var claims = new[]
            //    {
            //    new Claim(JwtRegisteredClaimNames.Sub, userInfo.Id.ToString()),
            //    new Claim(JwtRegisteredClaimNames.Email, userInfo.Email),
            //    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            //    new Claim("RoleId", userInfo.RoleId.ToString()),
            //    new Claim("FirstName", userInfo.FirstName),
            //    new Claim("LastName", userInfo.LastName)
            //};
            var claims = new[]
            {
                new Claim(ClaimTypes.Email, userInfo.Email),
                new Claim(ClaimTypes.Name, userInfo.FirstName +" "+ userInfo.LastName),
                new Claim(ClaimTypes.NameIdentifier, userInfo.Id.ToString())
            };


            var token = new JwtSecurityToken(
                issuer: AppSettings.JWTSettings?.JwtIssuer,
                audience: AppSettings.JWTSettings?.JwtAudience,
                claims: claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        //public async Task<IActionResult> CheckTokenAndRedirect()
        //{
        //    var httpContext = _httpContextAccessor.HttpContext;
        //    if (httpContext != null)
        //    {
        //        if (httpContext.Request.Cookies.TryGetValue("RememberMe_Token", out var token))
        //        {
        //            if (ValidateToken(token))
        //            {
        //                // Redirect to the admin page if the token is valid
        //                return new RedirectToPageResult("/Admin/Index");
        //            }
        //        }
        //    }
        //    // Return null or a default action if the token is not valid or does not exist
        //    return null;
        //}

        public bool ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSecretKey);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = AppSettings.JWTSettings.JwtIssuer,
                    ValidAudience = AppSettings.JWTSettings.JwtAudience,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                // Token validation failed
                return false;
            }
        }
        //public int GetCurrentUserId()
        //{
        //    //var httpContext = _httpContextAccessor.HttpContext;
        //    //if (httpContext?.User?.Identity?.IsAuthenticated == true)
        //    //{
        //    //    var userIdClaim = httpContext.User.Claims.FirstOrDefault(c => c.Type == "UserId");
        //    //    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        //    //    {
        //    //        return userId;
        //    //    }
        //    //}
        //    //throw new UnauthorizedAccessException("User is not authenticated or userId not found");

        //    var userInfo = GetCurrentUser();
        //    if (userInfo != null)
        //    {
        //        return userInfo.Id;
        //    }
        //    else
        //    {
        //        return 0;
        //    }
        //}


        //public UserLoginValidateDTO GetUserFromToken()
        //{
        //    var user = new UserLoginValidateDTO();
        //    var token = _httpContextAccessor.HttpContext?.Request.Cookies["RememberMe_Token"];
        //    if (!string.IsNullOrEmpty(token))
        //    {
        //        try
        //        {
        //            var tokenHandler = new JwtSecurityTokenHandler();
        //            var jwtToken = tokenHandler.ReadJwtToken(token);

        //            user.Id = int.Parse(jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value ?? "0");
        //            user.Email = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value ?? string.Empty;
        //            user.FirstName = jwtToken.Claims.FirstOrDefault(c => c.Type == "FirstName")?.Value ?? string.Empty;
        //            user.LastName = jwtToken.Claims.FirstOrDefault(c => c.Type == "LastName")?.Value ?? string.Empty;
        //            user.RoleId = int.Parse(jwtToken.Claims.FirstOrDefault(c => c.Type == "RoleId")?.Value ?? "0");
        //        }
        //        catch (Exception ex)
        //        {
        //            // Handle exceptions, such as parsing errors or missing claims
        //            throw new InvalidOperationException("Failed to parse token and extract user information.", ex);
        //        }
        //    }
        //    return user;
        //}

    }

}
