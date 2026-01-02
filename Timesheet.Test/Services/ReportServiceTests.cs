using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Timesheet.Application.Services;
using Timesheet.Domain.Entities;
using Timesheet.Infrastructure;

namespace Timesheet.Test.Services
{
    public class ReportServiceTests
    {
        [SetUp]
        public void Setup()
        {
            // nothing to mock for ReportService (uses DbContext directly)
            // Moq is referenced to satisfy request; not required here.
        }

        private TimesheetDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<TimesheetDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new TimesheetDbContext(options);
        }

        [Test]
        public async Task GetEmployeeHoursSummary_ReturnsTotalsAndRespectsDateFilter()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var ctx = CreateContext(dbName);

            // create users
            var user1 = new User { Id = 1, Name = "Alice" , Email ="Alice@gmail.com", IsActive = true, Password="alice"};
            var user2 = new User { Id = 2, Name = "Bob", Email = "Bob@gmail.com", IsActive = true, Password = "bob" };
            ctx.Users.AddRange(user1, user2);

            // project
            var project = new ProjectCode { Id = 10, ProjectName = "P1", Code = "P1", ClientName = "C", IsBillable = true, IsActive = true };
            ctx.ProjectCodes.Add(project);

            // timesheet + entries + hours
            var ts1 = new TimesheetData { Id = 100, UserId = user1.Id, User = user1, WeekStartDate = DateOnly.FromDateTime(new DateTime(2025,1,1)), WeekEndDate = DateOnly.FromDateTime(new DateTime(2025,1,7)) };
            var entry1 = new TimesheetEntry { Id = 200, Timesheet = ts1, TimesheetId = ts1.Id, ProjectCode = project, ProjectCodeId = project.Id, Description = "Work" };
            var h1 = new TimesheetEntryHour { Id = 300, TimesheetEntry = entry1, TimesheetEntryId = entry1.Id, WorkDate = DateOnly.FromDateTime(new DateTime(2025,1,2)), HoursWorked = 5m };
            entry1.Hours = new[] { h1 };
            ts1.Entries = new[] { entry1 };

            var ts2 = new TimesheetData { Id = 101, UserId = user2.Id, User = user2, WeekStartDate = DateOnly.FromDateTime(new DateTime(2024,12,25)), WeekEndDate = DateOnly.FromDateTime(new DateTime(2024,12,31)) };
            var entry2 = new TimesheetEntry { Id = 201, Timesheet = ts2, TimesheetId = ts2.Id, ProjectCode = project, ProjectCodeId = project.Id, Description = "Work2" };
            var h2 = new TimesheetEntryHour { Id = 301, TimesheetEntry = entry2, TimesheetEntryId = entry2.Id, WorkDate = DateOnly.FromDateTime(new DateTime(2024,12,26)), HoursWorked = 8m };
            entry2.Hours = new[] { h2 };
            ts2.Entries = new[] { entry2 };

            ctx.TimesheetDatas.AddRange(ts1, ts2);
            ctx.TimesheetEntries.AddRange(entry1, entry2);
            ctx.TimesheetEntryHours.AddRange(h1, h2);

            await ctx.SaveChangesAsync();

            var svc = new ReportService(ctx);

            // no filter - both users
            var all = await svc.GetEmployeeHoursSummary();
            Assert.AreEqual(2, all.Count);
            var alice = all.FirstOrDefault(x => x.UserId == user1.Id);
            var bob = all.FirstOrDefault(x => x.UserId == user2.Id);
            Assert.IsNotNull(alice);
            Assert.IsNotNull(bob);
            Assert.AreEqual(5m, alice.TotalHours);
            Assert.AreEqual(8m, bob.TotalHours);

            // filter to include only first week
            var filtered = await svc.GetEmployeeHoursSummary(DateOnly.FromDateTime(new DateTime(2025,1,1)), DateOnly.FromDateTime(new DateTime(2025,1,7)));
            Assert.AreEqual(1, filtered.Count);
            Assert.AreEqual(user1.Id, filtered[0].UserId);
            Assert.AreEqual(5m, filtered[0].TotalHours);
        }

        [Test]
        public async Task GetProjectHoursSummary_ReturnsProjectTotals()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var ctx = CreateContext(dbName);

            var user = new User { Id = 11, Name = "U", Email = "u@gmail.com", IsActive = true, Password = "u" };
            ctx.Users.Add(user);

            var p1 = new ProjectCode { Id = 21, Code = "P1", ProjectName = "Proj1", ClientName = "C1", IsBillable = true, IsActive = true };
            var p2 = new ProjectCode { Id = 22, Code = "P2", ProjectName = "Proj2", ClientName = "C2", IsBillable = false, IsActive = true };
            ctx.ProjectCodes.AddRange(p1, p2);

            var ts = new TimesheetData { UserId = user.Id, User = user, WeekStartDate = DateOnly.FromDateTime(new DateTime(2025,2,1)), WeekEndDate = DateOnly.FromDateTime(new DateTime(2025,2,7)) };
            var e1 = new TimesheetEntry { Timesheet = ts, ProjectCode = p1, ProjectCodeId = p1.Id, Description = "A" };
            var e2 = new TimesheetEntry { Timesheet = ts, ProjectCode = p2, ProjectCodeId = p2.Id, Description = "B" };
            ctx.TimesheetDatas.Add(ts);
            ctx.TimesheetEntries.AddRange(e1, e2);

            var h1 = new TimesheetEntryHour { TimesheetEntry = e1, WorkDate = DateOnly.FromDateTime(new DateTime(2025,2,2)), HoursWorked = 2.5m };
            var h2 = new TimesheetEntryHour { TimesheetEntry = e1, WorkDate = DateOnly.FromDateTime(new DateTime(2025,2,3)), HoursWorked = 3m };
            var h3 = new TimesheetEntryHour { TimesheetEntry = e2, WorkDate = DateOnly.FromDateTime(new DateTime(2025,2,2)), HoursWorked = 4m };
            ctx.TimesheetEntryHours.AddRange(h1, h2, h3);

            await ctx.SaveChangesAsync();

            var svc = new ReportService(ctx);

            var results = await svc.GetProjectHoursSummary();
            Assert.AreEqual(2, results.Count);
            var r1 = results.FirstOrDefault(r => r.ProjectCodeId == p1.Id);
            var r2 = results.FirstOrDefault(r => r.ProjectCodeId == p2.Id);
            Assert.IsNotNull(r1);
            Assert.IsNotNull(r2);
            Assert.AreEqual(5.5m, r1.TotalHours);
            Assert.AreEqual(4m, r2.TotalHours);
        }

        [Test]
        public async Task GetBillableHoursSummary_ReturnsBillableAndNonBillableTotals()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var ctx = CreateContext(dbName);

            var user = new User { Id = 50, Name = "U", Email = "u@gmail.com", IsActive = true, Password = "u" };
            ctx.Users.Add(user);

            var billable = new ProjectCode { Id = 60, Code = "B1", ProjectName = "Bill", ClientName = "C", IsBillable = true, IsActive = true };
            var non = new ProjectCode { Id = 61, Code = "N1", ProjectName = "Non", ClientName = "C", IsBillable = false, IsActive = true };
            ctx.ProjectCodes.AddRange(billable, non);

            var ts = new TimesheetData { UserId = user.Id, User = user, WeekStartDate = DateOnly.FromDateTime(new DateTime(2025,3,1)), WeekEndDate = DateOnly.FromDateTime(new DateTime(2025,3,7)) };
            var eB = new TimesheetEntry { Timesheet = ts, ProjectCode = billable, ProjectCodeId = billable.Id, Description = "A" };
            var eN = new TimesheetEntry { Timesheet = ts, ProjectCode = non, ProjectCodeId = non.Id, Description = "B" };
            ctx.TimesheetDatas.Add(ts);
            ctx.TimesheetEntries.AddRange(eB, eN);

            var hb = new TimesheetEntryHour { TimesheetEntry = eB, WorkDate = DateOnly.FromDateTime(new DateTime(2025,3,2)), HoursWorked = 6m };
            var hn = new TimesheetEntryHour { TimesheetEntry = eN, WorkDate = DateOnly.FromDateTime(new DateTime(2025,3,2)), HoursWorked = 2m };
            ctx.TimesheetEntryHours.AddRange(hb, hn);

            await ctx.SaveChangesAsync();

            var svc = new ReportService(ctx);

            var summary = await svc.GetBillableHoursSummary();
            Assert.AreEqual(2, summary.Count);
            var bill = summary.FirstOrDefault(s => s.IsBillable);
            var nonbill = summary.FirstOrDefault(s => !s.IsBillable);
            Assert.IsNotNull(bill);
            Assert.IsNotNull(nonbill);
            Assert.AreEqual(6m, bill.TotalHours);
            Assert.AreEqual(2m, nonbill.TotalHours);
        }
    }
}
