namespace Core.DTOs
{
    public class PageActionPermission
    {
        public bool CanView { get; set; }
        public bool CanAddEdit { get; set; }
        public bool CanDelete { get; set; }

        public static PageActionPermission None { get; } = new();

        public static PageActionPermission Full { get; } = new()
        {
            CanView = true,
            CanAddEdit = true,
            CanDelete = true
        };
    }
}
