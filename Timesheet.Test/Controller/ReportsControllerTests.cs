using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Timesheet.API.Controllers;
using Timesheet.Application.DTOs.Report;
using Timesheet.Application.Interfaces;

namespace Timesheet.Test.Controller
{
    public class ReportsControllerTests
    {
        private Mock<IReportService> _serviceMock = null!;

        [SetUp]
        public void Setup()
        {
            _serviceMock = new Mock<IReportService>();
        }

        [Test]
        public async Task GetEmployeeHours_ReturnsOkWithData()
        {
            var list = new List<EmployeeHoursSummaryDto>
            {
                new EmployeeHoursSummaryDto { UserId = 1, UserName = "A", TotalHours = 5m }
            };

            _serviceMock.Setup(s => s.GetEmployeeHoursSummary(It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>()))
                .ReturnsAsync(list);

            var controller = new ReportsController(_serviceMock.Object);

            var start = DateOnly.FromDateTime(new DateTime(2025,1,1));
            var end = DateOnly.FromDateTime(new DateTime(2025,1,7));

            var result = await controller.GetEmployeeHours(start, end);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var ok = result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreSame(list, ok!.Value);
            _serviceMock.Verify(s => s.GetEmployeeHoursSummary(start, end), Times.Once);
        }

        [Test]
        public async Task GetProjectHours_ReturnsOkWithData()
        {
            var list = new List<ProjectHoursSummaryDto>
            {
                new ProjectHoursSummaryDto { ProjectCodeId = 10, ProjectName = "P", IsBillable = true, TotalHours = 8m }
            };

            _serviceMock.Setup(s => s.GetProjectHoursSummary(It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>()))
                .ReturnsAsync(list);

            var controller = new ReportsController(_serviceMock.Object);

            var result = await controller.GetProjectHours(null, null);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var ok = result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreSame(list, ok!.Value);
            _serviceMock.Verify(s => s.GetProjectHoursSummary(null, null), Times.Once);
        }

        [Test]
        public async Task GetBillableHours_ReturnsOkWithData()
        {
            var list = new List<BillableHoursSummaryDto>
            {
                new BillableHoursSummaryDto { IsBillable = true, TotalHours = 12m },
                new BillableHoursSummaryDto { IsBillable = false, TotalHours = 3m }
            };

            _serviceMock.Setup(s => s.GetBillableHoursSummary(It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>()))
                .ReturnsAsync(list);

            var controller = new ReportsController(_serviceMock.Object);

            var start = DateOnly.FromDateTime(new DateTime(2025,3,1));

            var result = await controller.GetBillableHours(start, null);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var ok = result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreSame(list, ok!.Value);
            _serviceMock.Verify(s => s.GetBillableHoursSummary(start, null), Times.Once);
        }
    }
}
