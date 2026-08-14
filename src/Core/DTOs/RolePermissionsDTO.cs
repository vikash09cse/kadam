namespace Core.DTOs
{
    public class RolePermissionItemDTO
    {
        public int MenuId { get; set; }
        public bool CanAddEdit { get; set; }
        public bool CanDelete { get; set; }
    }

    public class RolePermissionsDTO
    {
        public int RoleId { get; set; }
        public List<int> PermissionIds { get; set; } = [];
        public List<RolePermissionItemDTO> Permissions { get; set; } = [];
    }
}
