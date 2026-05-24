using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class MenuPermission : BaseAuditableEntity
    {
        public string MenuName { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public string? MenuUrl { get; set; }
        public string? IconClass { get; set; }
        public int SortOrder { get; set; }
        public string? MenuKey { get; set; }
    }
}
