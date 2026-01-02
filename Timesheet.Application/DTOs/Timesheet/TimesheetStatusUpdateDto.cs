using System;

namespace Timesheet.Application.DTOs.Timesheet
{
    public class TimesheetStatusUpdateDto
    {
        // numeric status matching Timesheet.Domain.Enums.TimesheetStatus
        public int Status { get; set; }

        // required when rejecting
        public string? Comment { get; set; }
    }
}