namespace Core.DTOs;

/// <summary>
/// DTO returned by the /api/common/app-version endpoint.
/// Soft update uses LatestVersion; force update uses IsForceUpdate + MinimumRequiredVersion.
/// </summary>
public class AppVersionDTO
{
    /// <summary>Latest available version. Soft prompt when installed version is lower.</summary>
    public string LatestVersion { get; set; } = string.Empty;

    /// <summary>Minimum version required when IsForceUpdate is true.</summary>
    public string MinimumRequiredVersion { get; set; } = string.Empty;

    /// <summary>True if users on older versions must update before using the app.</summary>
    public bool IsForceUpdate { get; set; }

    /// <summary>Direct APK download URL hosted on the server.</summary>
    public string UpdateUrl { get; set; } = string.Empty;

    /// <summary>User-facing message shown in the update dialog.</summary>
    public string UpdateMessage { get; set; } = string.Empty;
}
