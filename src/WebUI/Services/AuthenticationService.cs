using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Core.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;
using Core.DTOs.Users;

public class AuthenticationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _jwtSecretKey;

    public AuthenticationService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _jwtSecretKey = AppSettings.JWTSettings?.JwtSecretKey ?? throw new InvalidOperationException("JWT secret key is not found in the configuration file.");
    }

    public async Task SignInUser(UserLoginValidateDTO userInfo, bool rememberMe)
    {
        var claims = new List<Claim>
        {
            new Claim("UserId", userInfo.Id.ToString()),
            new Claim(ClaimTypes.Name, $"{userInfo.FirstName} {userInfo.LastName}"),
            new Claim("Email", userInfo.Email),
            new Claim("RoleId", userInfo.RoleId.ToString()),
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = rememberMe
        };

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            try
            {
                await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                if (rememberMe)
                {
                    // Generate a secure token
                    var token = GenerateSecureToken(userInfo);

                    // Store the token in a secure cookie
                    httpContext.Response.Cookies.Append("RememberMe_Token", token, new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(7), Secure = true, HttpOnly = true });
                }
                else
                {
                    httpContext.Response.Cookies.Delete("RememberMe_Token");
                }
            }
            catch (Exception ex)
            {
                // Log exception and handle error
                throw new InvalidOperationException("An error occurred during sign-in.", ex);
            }
        }
    }

    private string GenerateSecureToken(UserLoginValidateDTO userInfo)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userInfo.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, userInfo.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("RoleId", userInfo.RoleId.ToString()),
            new Claim("FirstName", userInfo.FirstName),
            new Claim("LastName", userInfo.LastName)
        };

        var token = new JwtSecurityToken(
            issuer: AppSettings.JWTSettings.JwtIssuer,
            audience: AppSettings.JWTSettings.JwtAudience,
            claims: claims,
            expires: DateTime.Now.AddDays(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<IActionResult> CheckTokenAndRedirect()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            if (httpContext.Request.Cookies.TryGetValue("RememberMe_Token", out var token))
            {
                if (ValidateToken(token))
                {
                    // Redirect to the admin page if the token is valid
                    return new RedirectToPageResult("/Admin/Index");
                }
            }
        }
        // Return null or a default action if the token is not valid or does not exist
        return null;
    }

    private bool ValidateToken(string token)
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
    public int GetCurrentUserId()
    {
        //var httpContext = _httpContextAccessor.HttpContext;
        //if (httpContext?.User?.Identity?.IsAuthenticated == true)
        //{
        //    var userIdClaim = httpContext.User.Claims.FirstOrDefault(c => c.Type == "UserId");
        //    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        //    {
        //        return userId;
        //    }
        //}
        //throw new UnauthorizedAccessException("User is not authenticated or userId not found");

        var userInfo = GetCurrentUser();
        if (userInfo != null)
        {
            return userInfo.Id;
        }
        else
        {
            return 0;
        }
    }
    //public int? GetUserIdFromToken()
    //{
    //    var tokenHandler = new JwtSecurityTokenHandler();
    //    try
    //    {
    //        var token = _httpContextAccessor.HttpContext?.Request.Cookies["RememberMe_Token"];
    //        if (token != null)
    //        {
    //            var jwtToken = tokenHandler.ReadJwtToken(token);
    //            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
    //            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
    //            {
    //                return userId;
    //            }
    //        }
    //    }
    //    catch
    //    {
    //        return 0;
    //    }
    //    return 0;
    //}
    public async Task Logout()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            // Clear the authentication cookie
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // Delete the RememberMe_Token cookie
            httpContext.Response.Cookies.Delete("RememberMe_Token");
            // Check if the session is not null before clearing it
            if (httpContext.Session != null)
            {
                httpContext.Session.Clear();
            }
        }
    }

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
    public UserLoginValidateDTO? GetCurrentUser()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            throw new UnauthorizedAccessException("HttpContext is null");
        }

        // Check if the user is authenticated via JWT token
        var token = httpContext.Request.Cookies["RememberMe_Token"];
        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                return new UserLoginValidateDTO
                {
                    Id = int.Parse(jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value ?? "0"),
                    Email = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value ?? string.Empty,
                    FirstName = jwtToken.Claims.FirstOrDefault(c => c.Type == "FirstName")?.Value ?? string.Empty,
                    LastName = jwtToken.Claims.FirstOrDefault(c => c.Type == "LastName")?.Value ?? string.Empty,
                    RoleId = int.Parse(jwtToken.Claims.FirstOrDefault(c => c.Type == "RoleId")?.Value ?? "0")
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to parse token and extract user information.", ex);
            }
        }
        // Check if the user is authenticated via cookie
        else if (httpContext.User?.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = httpContext.User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return new UserLoginValidateDTO
                {
                    Id = userId,
                    Email = httpContext.User.Claims.FirstOrDefault(c => c.Type == "Email")?.Value ?? string.Empty,
                    FirstName = httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value.Split(' ')[0] ?? string.Empty,
                    LastName = httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value.Split(' ')[1] ?? string.Empty,
                    RoleId = int.Parse(httpContext.User.Claims.FirstOrDefault(c => c.Type == "RoleId")?.Value ?? "0")
                };
            }
        }
        return null;
        //throw new UnauthorizedAccessException("User is not authenticated or userId not found");
    }
}

