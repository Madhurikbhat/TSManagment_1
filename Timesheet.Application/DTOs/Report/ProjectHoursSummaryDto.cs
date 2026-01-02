using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timesheet.Application.DTOs.Report
{
    public class ProjectHoursSummaryDto
    {
        public int ProjectCodeId { get; set; }
        public string? ProjectName { get; set; }
        public bool IsBillable { get; set; }
        public decimal TotalHours { get; set; }
    }
}
