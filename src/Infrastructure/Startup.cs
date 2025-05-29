using Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public class InfrastructureStartup
{
}

public static class DependencyInject
{
    public static void InjectInfrastructure(this IServiceCollection services)
    {
        //services.AddScoped<IMailService, MailService>();
        services.AddScoped<IAdminRepository, AdminRepository>();
        services.AddScoped<IInstitutionRepository, InstitutionRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IStudentFamilyDetailsRepository, StudentFamilyDetailsRepository>();
        services.AddScoped<IStudentHealthRepository, StudentHealthRepository>();
        services.AddScoped<IStudentDocumentRepository, StudentDocumentRepository>();
        services.AddScoped<ITechnicianRepository, TechnicianRepository>();
        services.AddScoped<IStudentBaselineDetailRepository, StudentBaselineDetailRepository>();
        services.AddScoped<IStudentProgressRepository, StudentProgressRepository>();
        services.AddScoped<IDbSession, DbSession>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddDbContext<DatabaseContext>();
    }
}