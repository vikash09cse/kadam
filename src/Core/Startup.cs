using Core.Features.Admin;
using Microsoft.Extensions.DependencyInjection;

namespace Core;
public class CoreStartup
{
}

public static class DependencyInject
{
    public static void InjectCore(this IServiceCollection services)
    {
        services.AddScoped<AdminService>();
    }
}