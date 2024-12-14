﻿using Core.Abstractions;
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
        services.AddScoped<ITechnicianRepository, TechnicianRepository>();
        services.AddScoped<IDbSession, DbSession>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}