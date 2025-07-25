using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs.App
{
    public class DashboardDTO
    {
        public int ActiveCount { get; set; }
        public int CompletedCount { get; set; }
        public int InactiveCount { get; set; }
    }
}
