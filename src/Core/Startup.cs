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
        services.AddScoped<InstitutionService>();
        services.AddScoped<StudentService>();
        services.AddScoped<StudentFamilyDetailsService>();
        services.AddScoped<StudentHealthService>();
        services.AddScoped<StudentDocumentService>();
        services.AddScoped<StudentBaselineDetailService>();
        services.AddScoped<StudentProgressService>();
        services.AddScoped<ThemeActivityService>();
        services.AddScoped<StudentFollowupService>();
        services.AddScoped<StudentAttendanceService>();
    }
}