namespace Core.Entities
{
    public class Role : BaseAuditableEntity
    {
        public string RoleName { get; set; } = string.Empty;
        public Utilities.Enums.PortalType PortalType { get; set; } = Utilities.Enums.PortalType.Admin;
        /// <summary>When true, people with this role are assigned multiple divisions instead of institutions.</summary>
        public bool AllowMultipleDivision { get; set; }
    }
}
