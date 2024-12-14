using Infrastructure;
using Core;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace WebUI
{
    public class Startup
    {
    }
    public static class DependencyInject
    {
        public static void AddServices(this WebApplicationBuilder builder)
        {
            if (builder is null)
            {
                return;
            }
            //builder.Services.AddRazorPages(options =>
            //{
            //    options.Conventions.AuthorizeFolder("/Admin");
            //});

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "LoginCookies";
                options.Cookie.HttpOnly = false;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.LoginPath = "/Login";
                options.LogoutPath = "/Logout";
                options.AccessDeniedPath = "/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromDays(1);
                options.SlidingExpiration = true;


                });

            builder.Services.AddScoped<AuthenticationService>();
            builder.Services.InjectCore();
            builder.Services.InjectInfrastructure();
        }
    }
}
