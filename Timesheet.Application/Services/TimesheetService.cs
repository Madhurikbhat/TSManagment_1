using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Timesheet.Application.DTOs;
using Timesheet.Application.DTOs.Timesheet;
using Timesheet.Application.Interfaces;
using Timesheet.Domain.Entities;
using Timesheet.Domain.Enums;
using Timesheet.Infrastructure;

namespace Timesheet.Application.Services
{
    public class TimesheetService : ITimesheetService
    {
        private readonly TimesheetDbContext _context;
        private readonly IMapper _mapper;
        public TimesheetService(TimesheetDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<TimesheetDto>> GetAllTimesheetEntries()
        {
            var entities = await _context.TimesheetDatas
                .Include(t => t.Entries)
                .ThenInclude(e => e.Hours)
                .ToListAsync();

            return entities.Select(p => _mapper.Map<TimesheetDto>(p)).ToList();
        }

        public async Task<TimesheetDto> CreateEntry(CreateTimesheetDto dto)
        {
            // Map incoming DTO to entity graph
            var entity = _mapper.Map<TimesheetData>(dto);

            // Build set of incoming (projectId, date) pairs to check duplicates
            var incomingPairs = entity.Entries?
                .SelectMany(en => (en.Hours ?? new List<TimesheetEntryHour>())
                    .Select(h => new { ProjectId = en.ProjectCodeId, Date = h.WorkDate }))
                .ToList();

            if (incomingPairs != null && incomingPairs.Any())
            {
                var projectIds = incomingPairs.Select(p => (int)p.ProjectId).Distinct().ToList();
                var dates = incomingPairs.Select(p => (DateOnly)p.Date).Distinct().ToList();

                // Query existing hours for the same user, projects and dates
                var existingPairs = await _context.TimesheetEntryHours
                    .Include(h => h.TimesheetEntry)
                    .Where(h =>
                        h.TimesheetEntry.Timesheet.UserId == dto.UserId &&
                        projectIds.Contains(h.TimesheetEntry.ProjectCodeId) &&
                        dates.Contains(h.WorkDate))
                    .Select(h => new { ProjectId = h.TimesheetEntry.ProjectCodeId, Date = h.WorkDate })
                    .AsNoTracking()
                    .ToListAsync();

                var existingSet = new HashSet<(int, DateOnly)>(existingPairs.Select(x => (x.ProjectId, x.Date)));

                // Filter out hours that already exist
                foreach (var entry in entity.Entries ?? Enumerable.Empty<TimesheetEntry>())
                {
                    if (entry.Hours == null) continue;
                    entry.Hours = entry.Hours
                        .Where(h => !existingSet.Contains((entry.ProjectCodeId, h.WorkDate)))
                        .ToList();
                }

                // Remove entries that have no hours left after filtering
                entity.Entries = (entity.Entries ?? new List<TimesheetEntry>())
                    .Where(en => en.Hours != null && en.Hours.Any())
                    .ToList();
            }

            // If nothing remains to save, try to return an existing timesheet for that week (if present)
            if (entity.Entries == null || !entity.Entries.Any())
            {
                var existingTimesheet = await _context.TimesheetDatas
                    .Include(t => t.Entries)
                        .ThenInclude(e => e.Hours)
                    .FirstOrDefaultAsync(t =>
                        t.UserId == dto.UserId &&
                        t.WeekStartDate == dto.WeekStartDate &&
                        t.WeekEndDate == dto.WeekEndDate);

                if (existingTimesheet != null)
                    return _mapper.Map<TimesheetDto>(existingTimesheet);

                // Nothing to create and no existing timesheet found — return mapped (unsaved) entity to caller
                return _mapper.Map<TimesheetDto>(entity);
            }

            // Persist only the remaining (non-duplicate) entries/hours
            _context.TimesheetDatas.Add(entity);
            await _context.SaveChangesAsync();

            return _mapper.Map<TimesheetDto>(entity);
        }

        public async Task<List<TimesheetDto>> GetTimesheetsByStatus(int status)
        {
            var entities = await _context.TimesheetDatas
                 .Where(t => t.Entries.Any(e => (int)e.Status == status))
                 .Include(t => t.User)
                 .Include(t => t.Entries)
                     .ThenInclude(e => e.ProjectCode)
                 .Include(t => t.Entries)
                     .ThenInclude(e => e.Hours)
                 .AsNoTracking()
                 .ToListAsync();

            return entities.Select(p => _mapper.Map<TimesheetDto>(p)).ToList();
        }

        public async Task<TimesheetDto?> UpdateStatus(int id, int status, string? comment)
        {
            // validate status value
            if (!System.Enum.IsDefined(typeof(TimesheetStatus), status))
            {
                throw new System.ArgumentException("Invalid status value.", nameof(status));
            }

            var entry = await _context.TimesheetEntries
                 .Include(e => e.Timesheet)
                     .ThenInclude(t => t.Entries)
                         .ThenInclude(en => en.Hours)
                 .FirstOrDefaultAsync(e => e.Id == id);

            if (entry == null) return null;

            var newStatus = (TimesheetStatus)status;
            entry.Status = newStatus;

            // set comment only for rejected status
            if (newStatus == TimesheetStatus.Rejected)
            {
                entry.Comment = comment;
            }

            await _context.SaveChangesAsync();

            // map and return parent timesheet (contains the updated entry)
            var parentTimesheet = entry.Timesheet;
            return parentTimesheet == null ? null : _mapper.Map<TimesheetDto>(parentTimesheet);
        }

        // New: update all editable properties of an entry (project, description, status, comment, hours)
        public async Task<TimesheetDto?> UpdateEntryFully(int id, TimesheetEntryDto dto)
        {
            if (!System.Enum.IsDefined(typeof(TimesheetStatus), dto.Status))
                throw new ArgumentException("Invalid status value.", nameof(dto.Status));

            var entry = await _context.TimesheetEntries
                .Include(e => e.Timesheet)
                    .ThenInclude(t => t.Entries)
                        .ThenInclude(en => en.Hours)
                .Include(e => e.Hours)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (entry == null) return null;

            // update scalar fields
            entry.ProjectCodeId = dto.ProjectCodeId;
            entry.Description = dto.Description ?? string.Empty;
            entry.Status = (TimesheetStatus)dto.Status;
            entry.Comment = dto.Comment;

            // replace hours: remove existing and add incoming
            if (entry.Hours != null && entry.Hours.Any())
            {
                _context.TimesheetEntryHours.RemoveRange(entry.Hours);
            }

            entry.Hours = dto.Hours?.Select(h => new TimesheetEntryHour
            {
                TimesheetEntryId = entry.Id,
                WorkDate = h.Date,
                HoursWorked = h.Hours
            }).ToList() ?? new List<TimesheetEntryHour>();

            if (entry.Hours.Any())
            {
                await _context.TimesheetEntryHours.AddRangeAsync(entry.Hours);
            }

            await _context.SaveChangesAsync();

            var parentTimesheet = entry.Timesheet;
            return parentTimesheet == null ? null : _mapper.Map<TimesheetDto>(parentTimesheet);
        }

        public async Task<List<ProjectWeeklyEntryDto>> GetWeeklyEntriesForUser(int userId, DateOnly weekStart, DateOnly weekEnd)
        {
            if (weekEnd < weekStart) return new List<ProjectWeeklyEntryDto>();

            // materialize entry hours for the user in the date range
            var hours = await _context.TimesheetEntryHours
                .AsNoTracking()
                .Include(h => h.TimesheetEntry)
                    .ThenInclude(e => e.ProjectCode)
                .Include(h => h.TimesheetEntry)
                    .ThenInclude(e => e.Timesheet)
                .Where(h => h.TimesheetEntry.Timesheet.UserId == userId
                            && h.WorkDate >= weekStart
                            && h.WorkDate <= weekEnd)
                .ToListAsync();

            // build the list of dates in the range (inclusive)
            var totalDays = (weekEnd.ToDateTime(TimeOnly.MinValue) - weekStart.ToDateTime(TimeOnly.MinValue)).Days + 1;
            var dates = Enumerable.Range(0, totalDays)
                                  .Select(offset => weekStart.AddDays(offset))
                                  .ToArray();

            var grouped = hours
                .GroupBy(h => new
                {
                    EntryId = h.TimesheetEntry.Id,
                    ProjectCodeId = h.TimesheetEntry.ProjectCodeId,
                    ProjectName = h.TimesheetEntry.ProjectCode.ProjectName,
                    Description = h.TimesheetEntry.Description,
                    Status = h.TimesheetEntry.Status
                })
                .Select(g =>
                {
                    var dto = new ProjectWeeklyEntryDto
                    {
                        EntryId = g.Key.EntryId,
                        ProjectCodeId = g.Key.ProjectCodeId,
                        ProjectName = g.Key.ProjectName,
                        Description = g.Key.Description,
                        Status = (int)g.Key.Status
                    };

                    // For each date in the week, sum hours for that entry
                    foreach (var d in dates)
                    {
                        var sum = g.Where(x => x.WorkDate == d).Sum(x => x.HoursWorked);
                        dto.DailyHours.Add(new DailyHoursDto
                        {
                            Date = d,
                            Hours = sum
                        });
                    }

                    return dto;
                })
                .OrderBy(x => x.ProjectName)
                .ToList();

            return grouped;
        }
    }
}