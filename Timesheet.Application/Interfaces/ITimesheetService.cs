using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Application.DTOs;
using Timesheet.Application.DTOs.Timesheet;

namespace Timesheet.Application.Interfaces
{
    public interface ITimesheetService
    {
        Task<List<TimesheetDto>> GetAllTimesheetEntries();
        Task<TimesheetDto> CreateEntry(CreateTimesheetDto dto);
        Task<List<TimesheetDto>> GetTimesheetsByStatus(int status);
        Task<TimesheetDto?> UpdateStatus(int id, int status, string? comment);
        Task<TimesheetDto?> UpdateEntryFully(int id, TimesheetEntryDto dto);
        Task<List<ProjectWeeklyEntryDto>> GetWeeklyEntriesForUser(int userId, DateOnly weekStart, DateOnly weekEnd);

    }
}
