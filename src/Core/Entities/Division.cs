using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Division
    {
        public int Id { get; set; }
        public string DivisionName { get; set; }
        public string DivisionCode { get; set; }
        public int? StateId { get; set; }
        public byte DivisionStatus { get; set; }
        public DateTime? CloseDate { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DateCreated { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifyDate { get; set; }
        public int? ModifyBy { get; set; }
        public DateTime? DeletedDate { get; set; }
        public int? DeletedBy { get; set; }
    }
}
