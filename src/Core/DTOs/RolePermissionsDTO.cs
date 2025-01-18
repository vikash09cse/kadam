namespace Core.DTOs
{
    public class RolePermissionsDTO
    {
        public int RoleId { get; set; }
        public List<int> PermissionIds { get; set; }
    }
}
