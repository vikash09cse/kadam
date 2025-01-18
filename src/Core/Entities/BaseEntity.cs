using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public Utilities.Enums.Status CurrentStatus { get; set; } = Utilities.Enums.Status.Active;
        public bool IsDeleted { get; set; } = false;
    }

    public abstract class BaseAuditableEntity : BaseEntity
    {
        public int CreatedBy { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public int? ModifyBy { get; set; }
        public DateTime? ModifyDate { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
    }
}
