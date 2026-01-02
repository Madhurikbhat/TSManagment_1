using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timesheet.Application.DTOs.Timesheet
{
    public class TimesheetDto
    {
        public int Id { get; set; }

        public DateOnly WeekStartDate { get; set; }
        public DateOnly WeekEndDate { get; set; }

        public string UserName { get; set; } = null!;

        public List<TimesheetEntryDto> Entries { get; set; }
    }
}
