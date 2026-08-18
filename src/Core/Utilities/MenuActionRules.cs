namespace Core.Utilities
{
    public static class MenuActionRules
    {
        private static readonly HashSet<string> ViewOnlyUrls = new(StringComparer.OrdinalIgnoreCase)
        {
            "/Admin",
            "/Admin/Index",
            "/Admin/Report",
            "/Admin/AttendanceReport",
            "/Admin/ThemeActivityReport",
            "/StudentPortal/Dashboard"
        };

        private static readonly Dictionary<string, string> PathAliases = new(StringComparer.OrdinalIgnoreCase)
        {
            ["/Admin/AssignDivision"] = "/Admin/Peoples",
            ["/Admin/AssignInstitution"] = "/Admin/Peoples",
            ["/Admin/UserMenuPermissions"] = "/Admin/Peoples",
            ["/Admin/InstitutionBulkImport"] = "/Admin/Institutions"
        };

        public static string NormalizeMenuUrl(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            var url = path.Trim();
            var queryIndex = url.IndexOf('?', StringComparison.Ordinal);
            if (queryIndex >= 0)
            {
                url = url[..queryIndex];
            }

            url = url.TrimEnd('/');
            if (url.Length == 0)
            {
                return "/";
            }

            var segments = url.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length >= 3
                && segments[0].Equals("Admin", StringComparison.OrdinalIgnoreCase)
                && segments[1].Equals("UserMenuPermissions", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(segments[2], out _))
            {
                return "/Admin/Peoples";
            }

            if (PathAliases.TryGetValue(url, out var alias))
            {
                return alias;
            }

            return url.StartsWith('/') ? url : "/" + url;
        }

        public static bool HasActions(string? menuUrl)
        {
            var url = NormalizeMenuUrl(menuUrl);
            if (string.IsNullOrWhiteSpace(url) || url.StartsWith('#'))
            {
                return false;
            }

            if (!url.StartsWith('/'))
            {
                return false;
            }

            return !ViewOnlyUrls.Contains(url);
        }
    }
}
