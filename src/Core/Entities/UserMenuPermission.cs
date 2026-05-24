namespace Core.Entities
{
    public class UserMenuPermission : BaseAuditableEntity
    {
        public int UserId { get; set; }
        public int MenuId { get; set; }
    }
}
