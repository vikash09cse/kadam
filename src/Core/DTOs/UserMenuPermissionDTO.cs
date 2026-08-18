using Core.Utilities;

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
        public string? MenuUrl { get; set; }
        public bool IsSelected { get; set; }
        public bool IsInherited { get; set; }
        public bool CanAddEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool HasActions => MenuActionRules.HasActions(MenuUrl);
    }

    public class UserMenuPermissionSaveItemDTO
    {
        public int MenuId { get; set; }
        public bool CanAddEdit { get; set; }
        public bool CanDelete { get; set; }
    }

    public class UserMenuPermissionsDTO
    {
        public int UserId { get; set; }
        public List<int> MenuIds { get; set; } = [];
        public List<UserMenuPermissionSaveItemDTO> Permissions { get; set; } = [];
    }
}
