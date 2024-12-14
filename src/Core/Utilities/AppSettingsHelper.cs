using Core.DTOs;
using Microsoft.Extensions.Configuration;

namespace Core.Utilities;

public class AppSettingsHelper
{
	public AppSettingsHelper()
	{
        _Init();
    }
    private static IConfiguration _configuration = _Init();

    public static AppSettings.ConnectionStringsSection? ConnectionStrings()
    {
        return _GetSection<AppSettings.ConnectionStringsSection>("ConnectionStrings");
    }
    public static AppSettings.MailSettingsSection? MailSettings()
    {
        return _GetSection<AppSettings.MailSettingsSection>("MailSettings");
    }
    public static AppSettings.JWTSettingsSection? JWTSettings()
    {
        return _GetSection<AppSettings.JWTSettingsSection>("JWTSettings");
    }
    private static IConfiguration _Init()
    {
        var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        var projectRoot = Directory.GetCurrentDirectory();
        var jsonFile = isDevelopment
            ? "appsettings.Development.json"
            : "appsettings.json";

        return new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(projectRoot, jsonFile))
            .Build();
    }
    private static T? _GetSection<T>(string sectionName)
    {
        var section = _configuration.GetSection(sectionName);
        var value = section.Get<T>();

        return value;
    }
}

