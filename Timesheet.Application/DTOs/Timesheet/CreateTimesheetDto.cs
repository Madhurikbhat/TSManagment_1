using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timesheet.Application.DTOs.Timesheet
{
    public class CreateTimesheetDto
    {
        public int UserId { get; set; }
        public DateOnly WeekStartDate { get; set; }
        public DateOnly WeekEndDate { get; set; }
       
        public List<CreateTimesheetEntryDto> Entries { get; set; } = new();
    }
}
