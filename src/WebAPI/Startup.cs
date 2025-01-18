using Infrastructure;
using Core;
using Microsoft.AspNetCore.Authentication;

namespace WebAPI
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
            

           

            builder.Services.AddScoped<AuthenticationService>();
            builder.Services.InjectCore();
            builder.Services.InjectInfrastructure();
        }
    }
}
