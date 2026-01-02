using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Domain.Enums;

namespace Timesheet.Domain.Entities
{
    public class TimesheetData 
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public DateOnly WeekStartDate { get; set; }
        public DateOnly WeekEndDate { get; set; }
        
        public ICollection<TimesheetEntry> Entries { get; set; }
    }
}
