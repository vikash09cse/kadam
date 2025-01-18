using Core.Entities;
using System;

namespace Kadam.Core.Entities
{
    public class Section : BaseAuditableEntity
    {
        public string SectionName { get; set; }
    }
}