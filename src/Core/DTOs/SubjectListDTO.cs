using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Utilities.Enums;

namespace Core.DTOs
{
    public class SubjectListDTO
    {
        public int RowNumber { get; set; }
        public int Id { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public Status CurrentStatus { get; set; } = Status.Active;
        public string CurrentStatusText => CurrentStatus.ToString();
        public int TotalCount { get; set; } = 0;
    }
}
