namespace Core.Entities
{
    public class RolePermission : BaseAuditableEntity
    {
        public int RoleId { get; set; }
        public int MenuId { get; set; }
    }
}
