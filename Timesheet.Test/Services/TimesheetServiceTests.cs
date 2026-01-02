using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Timesheet.Application.DTOs.Timesheet;
using Timesheet.Application.Services;
using Timesheet.Domain.Entities;
using Timesheet.Domain.Enums;
using Timesheet.Infrastructure;

namespace Timesheet.Test.Services
{
    public class TimesheetServiceTests
    {
        private Mock<IMapper> _mapperMock = null!;

        [SetUp]
        public void Setup()
        {
            _mapperMock = new Mock<IMapper>();

            // Map TimesheetData -> TimesheetDto
            _mapperMock.Setup(m => m.Map<TimesheetDto>(It.IsAny<TimesheetData>()))
                .Returns((TimesheetData src) => new TimesheetDto
                {
                    Id = src.Id,
                    WeekStartDate = src.WeekStartDate,
                    WeekEndDate = src.WeekEndDate,
                    UserName = src.User?.Name ?? $"User{src.UserId}",
                    Entries = src.Entries?.Select(e => new TimesheetEntryDto
                    {
                        Id = e.Id,
                        TimesheetId = e.TimesheetId,
                        ProjectCodeId = e.ProjectCodeId,
                        ProjectName = e.ProjectCode?.ProjectName ?? string.Empty,
                        Description = e.Description,
                        Status = (int)e.Status,
                        Comment = e.Comment,
                        Hours = e.Hours?.Select(h => new TimesheetEntryHourDto { Date = h.WorkDate, Hours = h.HoursWorked }).ToList() ?? new List<TimesheetEntryHourDto>()
                    }).ToList() ?? new List<TimesheetEntryDto>()
                });

            // Map CreateTimesheetDto -> TimesheetData
            _mapperMock.Setup(m => m.Map<TimesheetData>(It.IsAny<CreateTimesheetDto>()))
                .Returns((CreateTimesheetDto dto) => new TimesheetData
                {
                    UserId = dto.UserId,
                    WeekStartDate = dto.WeekStartDate,
                    WeekEndDate = dto.WeekEndDate,
                    Entries = dto.Entries?.Select(e => new TimesheetEntry
                    {
                        ProjectCodeId = e.ProjectCodeId,
                        Description = e.Description,
                        Comment = e.Comment,
                        Status = (TimesheetStatus)e.Status,
                        Hours = e.Hours?.Select(h => new TimesheetEntryHour { WorkDate = h.Date, HoursWorked = h.Hours }).ToList() ?? new List<TimesheetEntryHour>()
                    }).ToList() ?? new List<TimesheetEntry>()
                });
        }

        private TimesheetDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<TimesheetDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new TimesheetDbContext(options);
        }

        [Test]
        public async Task GetAllTimesheetEntries_ReturnsMappedDtos()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var ctx = CreateContext(dbName);

            var ts = new TimesheetData
            {
                Id = 1,
                UserId = 10,
                WeekStartDate = DateOnly.FromDateTime(new DateTime(2025, 12, 22)),
                WeekEndDate = DateOnly.FromDateTime(new DateTime(2025, 12, 28)),
                Entries = new List<TimesheetEntry>
                {
                    new TimesheetEntry
                    {
                        Id = 100,
                        ProjectCodeId = 5,
                        Description = "Work",
                        Status = TimesheetStatus.Draft,
                        Hours = new List<TimesheetEntryHour>
                        {
                            new TimesheetEntryHour { Id = 1000, WorkDate = DateOnly.FromDateTime(new DateTime(2025,12,22)), HoursWorked = 8m }
                        }
                    }
                }
            };

            ctx.TimesheetDatas.Add(ts);
            await ctx.SaveChangesAsync();

            var service = new TimesheetService(ctx, _mapperMock.Object);

            var result = await service.GetAllTimesheetEntries();

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ts.Id, result[0].Id);
            Assert.IsNotNull(result[0].Entries);
            Assert.AreEqual(1, result[0].Entries.Count);
            Assert.AreEqual(100, result[0].Entries[0].Id);
        }

        [Test]
        public async Task UpdateStatus_WithValidStatus_UpdatesAndReturnsParent()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var ctx = CreateContext(dbName);

            var ts = new TimesheetData { Id = 11, UserId = 21, WeekStartDate = DateOnly.FromDateTime(new DateTime(2025,12,22)), WeekEndDate = DateOnly.FromDateTime(new DateTime(2025,12,28)), Entries = new List<TimesheetEntry>() };
            var entry = new TimesheetEntry { Id = 200, Timesheet = ts, TimesheetId = ts.Id, ProjectCodeId = 7, Description = "Task", Status = TimesheetStatus.Submitted, Hours = new List<TimesheetEntryHour>() };
            ts.Entries.Add(entry);
            ctx.TimesheetDatas.Add(ts);
            ctx.TimesheetEntries.Add(entry);
            await ctx.SaveChangesAsync();

            var service = new TimesheetService(ctx, _mapperMock.Object);

            var updated = await service.UpdateStatus(200, (int)TimesheetStatus.Approved, null);

            var dbEntry = await ctx.TimesheetEntries.FirstAsync(e => e.Id == 200);
            Assert.AreEqual(TimesheetStatus.Approved, dbEntry.Status);
            Assert.IsNotNull(updated);
            Assert.AreEqual(ts.Id, updated.Id);
        }

        [Test]
        public void UpdateStatus_WithInvalidStatus_Throws()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);
            var service = new TimesheetService(ctx, _mapperMock.Object);
            Assert.ThrowsAsync<ArgumentException>(async () => await service.UpdateStatus(1, 999, null));
        }

        [Test]
        public async Task GetWeeklyEntriesForUser_ReturnsGroupedDailySums()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var ctx = CreateContext(dbName);

            var project = new ProjectCode { Id = 50, ProjectName = "ProjA" , ClientName = "Clent 1", Code = "Proj.A"};
            ctx.ProjectCodes.Add(project);

            var ts = new TimesheetData { Id = 31, UserId = 7, WeekStartDate = DateOnly.FromDateTime(new DateTime(2025,12,22)), WeekEndDate = DateOnly.FromDateTime(new DateTime(2025,12,28)) };
            ctx.TimesheetDatas.Add(ts);

            var entry = new TimesheetEntry { Id = 401, Timesheet = ts, TimesheetId = ts.Id, ProjectCode = project, ProjectCodeId = project.Id, Description = "Desc", Status = TimesheetStatus.Draft, Hours = new List<TimesheetEntryHour>() };
            ctx.TimesheetEntries.Add(entry);

            var day1 = DateOnly.FromDateTime(new DateTime(2025,12,22));
            var day2 = DateOnly.FromDateTime(new DateTime(2025,12,23));

            var h1 = new TimesheetEntryHour { Id = 9001, TimesheetEntry = entry, TimesheetEntryId = entry.Id, WorkDate = day1, HoursWorked = 4m };
            var h2 = new TimesheetEntryHour { Id = 9002, TimesheetEntry = entry, TimesheetEntryId = entry.Id, WorkDate = day2, HoursWorked = 3.5m };
            ctx.TimesheetEntryHours.AddRange(h1, h2);

            await ctx.SaveChangesAsync();

            var service = new TimesheetService(ctx, _mapperMock.Object);

            var result = await service.GetWeeklyEntriesForUser(7, day1, day2);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            var row = result[0];
            Assert.AreEqual(entry.Id, row.EntryId);
            Assert.AreEqual(2, row.DailyHours.Count);
            var d1 = row.DailyHours.First(d => d.Date == day1);
            var d2 = row.DailyHours.First(d => d.Date == day2);
            Assert.AreEqual(4m, d1.Hours);
            Assert.AreEqual(3.5m, d2.Hours);
        }

        [Test]
        public async Task CreateEntry_WhenDuplicateHours_ReturnsExistingTimesheet()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var ctx = CreateContext(dbName);

            var tsExisting = new TimesheetData { Id = 88, UserId = 200, WeekStartDate = DateOnly.FromDateTime(new DateTime(2025,12,22)), WeekEndDate = DateOnly.FromDateTime(new DateTime(2025,12,28)), Entries = new List<TimesheetEntry>() };
            var entryExisting = new TimesheetEntry { Id = 700, Timesheet = tsExisting, TimesheetId = tsExisting.Id, ProjectCodeId = 300, Description = "Existing", Status = TimesheetStatus.Draft, Hours = new List<TimesheetEntryHour>() };
            var hourExisting = new TimesheetEntryHour { Id = 7000, TimesheetEntry = entryExisting, TimesheetEntryId = entryExisting.Id, WorkDate = DateOnly.FromDateTime(new DateTime(2025,12,22)), HoursWorked = 2m };
            entryExisting.Hours.Add(hourExisting);
            tsExisting.Entries.Add(entryExisting);

            ctx.TimesheetDatas.Add(tsExisting);
            ctx.TimesheetEntries.Add(entryExisting);
            ctx.TimesheetEntryHours.Add(hourExisting);
            await ctx.SaveChangesAsync();

            var createDto = new CreateTimesheetDto
            {
                UserId = 200,
                WeekStartDate = tsExisting.WeekStartDate,
                WeekEndDate = tsExisting.WeekEndDate,
                Entries = new List<CreateTimesheetEntryDto>
                {
                    new CreateTimesheetEntryDto
                    {
                        ProjectCodeId = 300,
                        Description = "New",
                        Status = (int)TimesheetStatus.Draft,
                        Hours = new List<TimesheetEntryHourDto>
                        {
                            new TimesheetEntryHourDto { Date = hourExisting.WorkDate, Hours = hourExisting.HoursWorked }
                        }
                    }
                }
            };

            var service = new TimesheetService(ctx, _mapperMock.Object);

            var returned = await service.CreateEntry(createDto);

            Assert.IsNotNull(returned);
            Assert.AreEqual(tsExisting.Id, returned.Id);
        }

        [Test]
        public async Task UpdateEntryFully_ReplacesHoursAndUpdatesFields()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var ctx = CreateContext(dbName);

            var ts = new TimesheetData { Id = 3000, UserId = 10, WeekStartDate = DateOnly.FromDateTime(new DateTime(2025,6,1)), WeekEndDate = DateOnly.FromDateTime(new DateTime(2025,6,7)), Entries = new List<TimesheetEntry>() };
            var entry = new TimesheetEntry { Id = 4000, Timesheet = ts, TimesheetId = ts.Id, ProjectCodeId = 5, Description = "Old", Status = TimesheetStatus.Draft, Hours = new List<TimesheetEntryHour>() };
            ts.Entries.Add(entry);
            ctx.TimesheetDatas.Add(ts);
            ctx.TimesheetEntries.Add(entry);
            ctx.TimesheetEntryHours.Add(new TimesheetEntryHour { TimesheetEntry = entry, TimesheetEntryId = entry.Id, WorkDate = DateOnly.FromDateTime(new DateTime(2025,6,1)), HoursWorked = 2m });
            await ctx.SaveChangesAsync();

            var dto = new TimesheetEntryDto
            {
                Id = entry.Id,
                TimesheetId = entry.TimesheetId,
                ProjectCodeId = 6,
                Description = "NewDesc",
                Status = (int)TimesheetStatus.Approved,
                Comment = "Ok",
                Hours = new List<TimesheetEntryHourDto>
                {
                    new TimesheetEntryHourDto { Date = DateOnly.FromDateTime(new DateTime(2025,6,2)), Hours = 4m }
                }
            };

            var service = new TimesheetService(ctx, _mapperMock.Object);

            var updated = await service.UpdateEntryFully(entry.Id, dto);

            Assert.IsNotNull(updated);
            var reloadedEntry = await ctx.TimesheetEntries.Include(e => e.Hours).FirstAsync(e => e.Id == entry.Id);
            Assert.AreEqual(6, reloadedEntry.ProjectCodeId);
            Assert.AreEqual(TimesheetStatus.Approved, reloadedEntry.Status);
            Assert.AreEqual(1, reloadedEntry.Hours.Count);
            Assert.AreEqual(4m, reloadedEntry.Hours.First().HoursWorked);
        }
    }
}
