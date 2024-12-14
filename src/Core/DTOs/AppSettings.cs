using Core.Utilities;

namespace Core.DTOs;

public class AppSettings
{
    static AppSettings()
    {
        ConnectionStrings = AppSettingsHelper.ConnectionStrings();
        MailSettings = AppSettingsHelper.MailSettings();
        JWTSettings = AppSettingsHelper.JWTSettings();
    }
    public static ConnectionStringsSection? ConnectionStrings { get; }
    public static MailSettingsSection? MailSettings { get; }
    public static JWTSettingsSection? JWTSettings { get; }
    public class ConnectionStringsSection
    {
        #region # Keys
        public string DBConnection { get; set; } = string.Empty;
        #endregion
    }

    public class MailSettingsSection
    {
        public bool IsEmailEnable { get; set; }
        public string? From { get; set; }
        public string? Cc { get; set; }
        public string? Bcc { get; set; }
        public string? Host { get; set; }
        public int Port { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public bool UseSSL { get; set; }
        public bool UseStartTls { get; set; }
    }

    public class JWTSettingsSection
    {
        public string JwtSecretKey { get; set; }
        public string JwtIssuer { get; set; }
        public string JwtAudience { get; set; }
    }
}
