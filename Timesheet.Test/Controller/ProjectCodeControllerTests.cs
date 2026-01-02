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
    public class ProjectCodeControllerTests
    {
        private Mock<IProjectCodeService> _serviceMock = null!;

        [SetUp]
        public void Setup()
        {
            _serviceMock = new Mock<IProjectCodeService>();
        }

        [Test]
        public async Task GetAll_ReturnsOkWithList()
        {
            var list = new List<ProjectCodeResponseDto>
            {
                new ProjectCodeResponseDto { Id = 1, Code = "C1", ProjectName = "P1", ClientName = "Cl1", IsActive = true, IsBillable = true }
            };
            _serviceMock.Setup(s => s.GetAllProjects()).ReturnsAsync(list);

            var controller = new ProjectCodeController(_serviceMock.Object);

            var result = await controller.GetAll();

            Assert.IsInstanceOf<OkObjectResult>(result);
            var ok = result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreSame(list, ok!.Value);
        }

        [Test]
        public async Task Create_NullDto_ReturnsBadRequest()
        {
            var controller = new ProjectCodeController(_serviceMock.Object);

            var result = await controller.Create(null!);

            Assert.IsInstanceOf<BadRequestResult>(result);
        }

        [Test]
        public async Task Create_ValidDto_CallsServiceAndReturnsCreated()
        {
            var dto = new ProjectCodeCreateDto { Code = "NEW", ProjectName = "NewProject", ClientName = "ClientX", IsActive = true, IsBillable = true };
            var response = new ProjectCodeResponseDto { Id = 5, Code = dto.Code, ProjectName = dto.ProjectName, ClientName = dto.ClientName, IsActive = dto.IsActive, IsBillable = dto.IsBillable };

            _serviceMock.Setup(s => s.CreateProject(It.IsAny<ProjectCodeCreateDto>())).ReturnsAsync(response);

            var controller = new ProjectCodeController(_serviceMock.Object);

            var result = await controller.Create(dto);

            Assert.IsInstanceOf<CreatedAtActionResult>(result);
            var created = result as CreatedAtActionResult;
            Assert.IsNotNull(created);
            Assert.AreEqual(response, created!.Value);
            _serviceMock.Verify(s => s.CreateProject(It.Is<ProjectCodeCreateDto>(d => d.Code == dto.Code && d.ProjectName == dto.ProjectName)), Times.Once);
        }

        [Test]
        public async Task Update_NullDto_ReturnsBadRequest()
        {
            var controller = new ProjectCodeController(_serviceMock.Object);

            var result = await controller.Update(1, null!);

            Assert.IsInstanceOf<BadRequestResult>(result);
        }

        [Test]
        public async Task Update_WhenNotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.UpdateProject(10, It.IsAny<ProjectCodeCreateDto>())).ReturnsAsync((ProjectCodeResponseDto?)null);

            var controller = new ProjectCodeController(_serviceMock.Object);

            var result = await controller.Update(10, new ProjectCodeCreateDto { Code = "x", ProjectName = "x" });

            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task Update_WhenExists_ReturnsOkWithDto()
        {
            var updateDto = new ProjectCodeCreateDto { Code = "U", ProjectName = "Updated", ClientName = "C", IsActive = false, IsBillable = true };
            var resp = new ProjectCodeResponseDto { Id = 20, Code = updateDto.Code, ProjectName = updateDto.ProjectName, ClientName = updateDto.ClientName, IsActive = updateDto.IsActive, IsBillable = updateDto.IsBillable };
            _serviceMock.Setup(s => s.UpdateProject(20, It.IsAny<ProjectCodeCreateDto>())).ReturnsAsync(resp);

            var controller = new ProjectCodeController(_serviceMock.Object);

            var result = await controller.Update(20, updateDto);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var ok = result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreEqual(resp, ok!.Value);
        }
    }
}
