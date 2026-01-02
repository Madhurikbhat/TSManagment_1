using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timesheet.Application.DTOs
{
    public class ProjectWeeklyEntryDto
    {
        public int EntryId { get; set; }
        public int ProjectCodeId { get; set; }
        public string ProjectName { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int Status { get; set; }

        // One item per day in the requested range, Date and total hours for that day
        public List<DailyHoursDto> DailyHours { get; set; } = new();
    }
}
