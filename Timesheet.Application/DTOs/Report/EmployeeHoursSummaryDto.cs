using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timesheet.Application.DTOs.Report
{
    public class EmployeeHoursSummaryDto
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public decimal TotalHours { get; set; }
    }
}
