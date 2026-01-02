using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Domain.Entities;

namespace Timesheet.Application.DTOs.Timesheet
{
    public class CreateTimesheetEntryDto
    {
        public int ProjectCodeId { get; set; }
        public string Description { get; set; }
        public string? Comment { get; set; }
        public int Status { get; set; }
        public List<TimesheetEntryHourDto> Hours { get; set; }
    }
}
