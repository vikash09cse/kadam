using Core.Entities;

namespace Kadam.Core.Entities
{
public class Grade: BaseAuditableEntity
    {
    public string Name { get; set; } = string.Empty;
    }
}