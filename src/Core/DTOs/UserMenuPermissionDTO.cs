namespace Core.DTOs
{
    public class NavigationMenuDTO
    {
        public int Id { get; set; }
        public string MenuName { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public string? MenuUrl { get; set; }
        public string? IconClass { get; set; }
        public string? MenuKey { get; set; }
        public int SortOrder { get; set; }
        public List<NavigationMenuDTO> Children { get; set; } = [];
    }

    public class UserMenuPermissionItemDTO
    {
        public int Id { get; set; }
        public string MenuName { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public string ParentMenuName { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsSelected { get; set; }
    }

    public class UserMenuPermissionsDTO
    {
        public int UserId { get; set; }
        public List<int> MenuIds { get; set; } = [];
    }
}
