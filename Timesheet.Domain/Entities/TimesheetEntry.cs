using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Domain.Enums;

namespace Timesheet.Domain.Entities
{
    public class TimesheetEntry
    {
        public int Id { get; set; }
        public int TimesheetId { get; set; }
        public TimesheetData Timesheet { get; set; } = null!;
        public int ProjectCodeId { get; set; }
        public ProjectCode ProjectCode { get; set; } = null!;
        public string Description { get; set; }
        public string? Comment { get; set; }
        public TimesheetStatus Status { get; set; }
        public ICollection<TimesheetEntryHour> Hours { get; set; }
    }
}
