using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Timesheet.API.Controllers;
using Timesheet.Application.DTOs;
using Timesheet.Application.Interfaces;

namespace Timesheet.Test.Controller
{
    public class ProjectAssignmentControllerTests
    {
        private Mock<IProjectAssignmentService> _serviceMock = null!;

        [SetUp]
        public void Setup()
        {
            _serviceMock = new Mock<IProjectAssignmentService>();
        }

        [Test]
        public async Task GetAll_ReturnsOkWithList()
        {
            var list = new List<ProjectAssignmentResponseDto>
            {
                new ProjectAssignmentResponseDto { Id = 1, UserId = 5, ProjectCodeId = 10, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1) }
            };
            _serviceMock.Setup(s => s.GetAllProjectAssignment()).ReturnsAsync(list);

            var controller = new ProjectAssignmentController(_serviceMock.Object);

            var result = await controller.GetAll();

            Assert.IsInstanceOf<OkObjectResult>(result);
            var ok = result as OkObjectResult;
            Assert.IsNotNull(ok!.Value);
            Assert.AreSame(list, ok.Value);
        }

        [Test]
        public async Task Create_NullDto_ReturnsBadRequest()
        {
            var controller = new ProjectAssignmentController(_serviceMock.Object);

            var result = await controller.Create(null!);

            Assert.IsInstanceOf<BadRequestResult>(result);
        }

        [Test]
        public async Task Create_ValidDto_CallsServiceAndReturnsCreated()
        {
            var dto = new ProjectAssignmentCreateDto { UserId = 7, ProjectCodeId = 15, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(5) };
            var response = new ProjectAssignmentResponseDto { Id = 55, UserId = dto.UserId, ProjectCodeId = dto.ProjectCodeId, StartDate = dto.StartDate, EndDate = dto.EndDate };

            _serviceMock.Setup(s => s.CreateProjectAssignment(It.IsAny<ProjectAssignmentCreateDto>())).ReturnsAsync(response);

            var controller = new ProjectAssignmentController(_serviceMock.Object);

            var result = await controller.Create(dto);

            Assert.IsInstanceOf<CreatedAtActionResult>(result);
            var created = result as CreatedAtActionResult;
            Assert.IsNotNull(created);
            Assert.AreEqual(response, created!.Value);
            _serviceMock.Verify(s => s.CreateProjectAssignment(It.Is<ProjectAssignmentCreateDto>(d => d.UserId == dto.UserId && d.ProjectCodeId == dto.ProjectCodeId)), Times.Once);
        }

        [Test]
        public async Task GetProjectsByUser_ReturnsOkWithProjects()
        {
            var projects = new List<ProjectCodeResponseDto>
            {
                new ProjectCodeResponseDto { Id = 100, Code = "P1", ProjectName = "Proj1", ClientName = "C", IsBillable = true, IsActive = true }
            };

            _serviceMock.Setup(s => s.GetProjectsByUserId(77)).ReturnsAsync(projects);

            var controller = new ProjectAssignmentController(_serviceMock.Object);

            var result = await controller.GetProjectsByUser(77);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var ok = result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreSame(projects, ok!.Value);
        }
    }
}
