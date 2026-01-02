using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Timesheet.API.Controllers;
using Timesheet.Application.DTOs;
using Timesheet.Application.DTOs.Timesheet;
using Timesheet.Application.Interfaces;
using Timesheet.Domain.Enums;

namespace Timesheet.Test.Controller
{
    public class TimesheetControllerTests
    {
        private Mock<ITimesheetService> _serviceMock = null!;

        [SetUp]
        public void Setup()
        {
            _serviceMock = new Mock<ITimesheetService>();
        }

        [Test]
        public async Task GetAll_ReturnsOkWithList()
        {
            var list = new List<TimesheetDto>
            {
                new TimesheetDto { Id = 1, WeekStartDate = DateOnly.FromDateTime(DateTime.Today), WeekEndDate = DateOnly.FromDateTime(DateTime.Today) }
            };
            _serviceMock.Setup(s => s.GetAllTimesheetEntries()).ReturnsAsync(list);

            var controller = new TimesheetController(_serviceMock.Object);

            var result = await controller.GetAll();

            Assert.IsInstanceOf<OkObjectResult>(result);
            var ok = result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreSame(list, ok!.Value);
        }

        [Test]
        public async Task GetTimesheets_ReturnsOkWithList()
        {
            var list = new List<TimesheetDto>();
            _serviceMock.Setup(s => s.GetTimesheetsByStatus(2)).ReturnsAsync(list);

            var controller = new TimesheetController(_serviceMock.Object);

            var result = await controller.GetTimesheets();

            Assert.IsInstanceOf<OkObjectResult>(result);
            var ok = result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreSame(list, ok!.Value);
        }

        [Test]
        public async Task Create_NullDto_ReturnsBadRequest()
        {
            var controller = new TimesheetController(_serviceMock.Object);

            var result = await controller.Create(null!);

            Assert.IsInstanceOf<BadRequestResult>(result);
        }

        [Test]
        public async Task Create_ValidDto_ReturnsCreated()
        {
            var createDto = new CreateTimesheetDto { UserId = 10, WeekStartDate = DateOnly.FromDateTime(DateTime.Today), WeekEndDate = DateOnly.FromDateTime(DateTime.Today) };
            var returned = new TimesheetDto { Id = 5, WeekStartDate = createDto.WeekStartDate, WeekEndDate = createDto.WeekEndDate };

            _serviceMock.Setup(s => s.CreateEntry(createDto)).ReturnsAsync(returned);

            var controller = new TimesheetController(_serviceMock.Object);

            var result = await controller.Create(createDto);

            Assert.IsInstanceOf<CreatedAtActionResult>(result);
            var created = result as CreatedAtActionResult;
            Assert.IsNotNull(created);
            Assert.AreEqual(returned, created!.Value);
            _serviceMock.Verify(s => s.CreateEntry(It.Is<CreateTimesheetDto>(d => d.UserId == createDto.UserId)), Times.Once);
        }

        [Test]
        public async Task UpdateStatus_NullDto_ReturnsBadRequest()
        {
            var controller = new TimesheetController(_serviceMock.Object);

            var result = await controller.UpdateStatus(1, null!);

            Assert.IsInstanceOf<BadRequestResult>(result);
        }

        [Test]
        public async Task UpdateStatus_RejectWithoutComment_ReturnsBadRequest()
        {
            var controller = new TimesheetController(_serviceMock.Object);
            var dto = new TimesheetStatusUpdateDto { Status = (int)TimesheetStatus.Rejected, Comment = null };

            var result = await controller.UpdateStatus(1, dto);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task UpdateStatus_ServiceThrows_ReturnsBadRequest()
        {
            var dto = new TimesheetStatusUpdateDto { Status = (int)TimesheetStatus.Approved, Comment = null };
            _serviceMock.Setup(s => s.UpdateStatus(1, dto.Status, dto.Comment)).ThrowsAsync(new ArgumentException("Invalid status"));

            var controller = new TimesheetController(_serviceMock.Object);

            var result = await controller.UpdateStatus(1, dto);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var bad = result as BadRequestObjectResult;
            Assert.IsNotNull(bad);
            Assert.AreEqual("Invalid status", bad!.Value);
        }

        [Test]
        public async Task UpdateStatus_Valid_ReturnsOk()
        {
            var dto = new TimesheetStatusUpdateDto { Status = (int)TimesheetStatus.Approved, Comment = null };
            var timesheet = new TimesheetDto { Id = 11 };
            _serviceMock.Setup(s => s.UpdateStatus(2, dto.Status, dto.Comment)).ReturnsAsync(timesheet);

            var controller = new TimesheetController(_serviceMock.Object);

            var result = await controller.UpdateStatus(2, dto);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var ok = result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreEqual(timesheet, ok!.Value);
        }

        [Test]
        public async Task UpdateEntryFully_NullDto_ReturnsBadRequest()
        {
            var controller = new TimesheetController(_serviceMock.Object);

            var result = await controller.UpdateEntryFully(1, null!);

            Assert.IsInstanceOf<BadRequestResult>(result);
        }

        [Test]
        public async Task UpdateEntryFully_RejectWithoutComment_ReturnsBadRequest()
        {
            var dto = new TimesheetEntryDto { Id = 1, Status = (int)TimesheetStatus.Rejected, Comment = null };
            var controller = new TimesheetController(_serviceMock.Object);

            var result = await controller.UpdateEntryFully(1, dto);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task UpdateEntryFully_Valid_ReturnsOk()
        {
            var dto = new TimesheetEntryDto { Id = 2, Status = (int)TimesheetStatus.Approved, Comment = "ok" };
            var ts = new TimesheetDto { Id = 99 };
            _serviceMock.Setup(s => s.UpdateEntryFully(2, dto)).ReturnsAsync(ts);

            var controller = new TimesheetController(_serviceMock.Object);

            var result = await controller.UpdateEntryFully(2, dto);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var ok = result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreEqual(ts, ok!.Value);
        }

        [Test]
        public async Task GetWeeklyEntriesForUser_ReturnsOkWithList()
        {
            var start = DateOnly.FromDateTime(DateTime.Today);
            var end = DateOnly.FromDateTime(DateTime.Today.AddDays(6));
            var list = new List<ProjectWeeklyEntryDto> { new ProjectWeeklyEntryDto { EntryId = 1 } };
            _serviceMock.Setup(s => s.GetWeeklyEntriesForUser(7, start, end)).ReturnsAsync(list);

            var controller = new TimesheetController(_serviceMock.Object);

            var result = await controller.GetWeeklyEntriesForUser(7, start, end);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var ok = result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreSame(list, ok!.Value);
        }
    }
}
