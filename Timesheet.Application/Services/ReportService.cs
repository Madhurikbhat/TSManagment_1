using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Application.DTOs.Report;
using Timesheet.Application.Interfaces;
using Timesheet.Domain.Entities;
using Timesheet.Infrastructure;

namespace Timesheet.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly TimesheetDbContext _context;
        public ReportService(TimesheetDbContext context)
        {
            _context = context;
        }
        private IQueryable<TimesheetEntryHour> HoursQuery()
        {
            var q = _context.TimesheetEntryHours
                .AsQueryable()
                .Include(h => h.TimesheetEntry)
                    .ThenInclude(e => e.Timesheet)
                        .ThenInclude(t => t.User)
                .Include(h => h.TimesheetEntry)
                    .ThenInclude(e => e.ProjectCode);
            return q;
        }

        public async Task<List<EmployeeHoursSummaryDto>> GetEmployeeHoursSummary(DateOnly? startDate = null, DateOnly? endDate = null)
        {
            var q = HoursQuery();
            if (startDate.HasValue)
                q = q.Where(h => h.WorkDate >= startDate.Value);

            if (endDate.HasValue)
                q = q.Where(h => h.WorkDate <= endDate.Value);

            var result = await q
                 .GroupBy(h => new { h.TimesheetEntry.Timesheet.UserId, h.TimesheetEntry.Timesheet.User.Name })
                 .Select(g => new EmployeeHoursSummaryDto
                 {
                     UserId = g.Key.UserId,
                     UserName = g.Key.Name,
                     TotalHours = g.Sum(x => x.HoursWorked)
                 })
                 .ToListAsync();

            return result;
        }
        public async Task<List<ProjectHoursSummaryDto>> GetProjectHoursSummary(DateOnly? startDate = null, DateOnly? endDate = null)
        {
            var q = HoursQuery();
            if (startDate.HasValue)
                q = q.Where(h => h.WorkDate >= startDate.Value);

            if (endDate.HasValue)
                q = q.Where(h => h.WorkDate <= endDate.Value);

            var result = await q
                .GroupBy(h => new { h.TimesheetEntry.ProjectCodeId, h.TimesheetEntry.ProjectCode.ProjectName, h.TimesheetEntry.ProjectCode.IsBillable })
                .Select(g => new ProjectHoursSummaryDto
                {
                    ProjectCodeId = g.Key.ProjectCodeId,
                    ProjectName = g.Key.ProjectName,
                    IsBillable = g.Key.IsBillable,
                    TotalHours = g.Sum(x => x.HoursWorked)
                })
                .ToListAsync();

            return result;
        }
        public async Task<List<BillableHoursSummaryDto>> GetBillableHoursSummary(DateOnly? startDate = null, DateOnly? endDate = null)
        {
            var q = HoursQuery();

            if (startDate.HasValue)
                q = q.Where(h => h.WorkDate >= startDate.Value);

            if (endDate.HasValue)
                q = q.Where(h => h.WorkDate <= endDate.Value);

            var result = await q
                .GroupBy(h => h.TimesheetEntry.ProjectCode.IsBillable)
                .Select(g => new BillableHoursSummaryDto
                {
                    IsBillable = g.Key,
                    TotalHours = g.Sum(x => x.HoursWorked)
                })
                .ToListAsync();

            return result;
        }
    }
}
