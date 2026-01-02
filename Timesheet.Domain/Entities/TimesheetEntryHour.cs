using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timesheet.Domain.Entities
{
    public class TimesheetEntryHour
    {
        public int Id { get; set; }

        public int TimesheetEntryId { get; set; }
        public TimesheetEntry TimesheetEntry { get; set; } = null!;

        public DateOnly WorkDate { get; set; }

        public decimal HoursWorked { get; set; } // 0.00 – 24.00
    }
}
