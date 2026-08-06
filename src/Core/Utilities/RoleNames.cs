namespace Core.Utilities
{
    public static class RoleNames
    {
        public const string DO = "DO";
        public const string SPM = "SPM";
        public const string Admin = "Admin";

        public static bool IsDivisionScopedRole(Core.Entities.Role? role)
        {
            if (role == null)
            {
                return false;
            }

            if (role.AllowMultipleDivision)
            {
                return true;
            }

            // Fallback for environments where the column is not yet backfilled
            var name = role.RoleName?.Trim() ?? string.Empty;
            return name.Equals(DO, StringComparison.OrdinalIgnoreCase)
                || name.Equals(SPM, StringComparison.OrdinalIgnoreCase);
        }
    }
}
