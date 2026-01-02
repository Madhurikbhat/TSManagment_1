using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timesheet.Application.DTOs.Report
{
    public class BillableHoursSummaryDto
    {
        public bool IsBillable { get; set; }
        public decimal TotalHours { get; set; }
    }
}
