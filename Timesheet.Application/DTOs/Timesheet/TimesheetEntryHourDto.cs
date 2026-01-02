using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timesheet.Application.DTOs.Timesheet
{
    public class TimesheetEntryHourDto
    {
        public DateOnly Date { get; set; }
        public decimal Hours { get; set; }
    }
}
