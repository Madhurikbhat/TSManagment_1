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
    public class UserControllerTests
    {
        private Mock<IUserService> _serviceMock = null!;

        [SetUp]
        public void Setup()
        {
            _serviceMock = new Mock<IUserService>();
        }

        [Test]
        public async Task GetAll_ReturnsOkWithUsers()
        {
            var users = new List<UserResponseDto>
            {
                new UserResponseDto { Id = 1, Name = "Alice", Role = 1, Email = "a@example.com", IsActive = true }
            };

            _serviceMock.Setup(s => s.GetAllUsers()).ReturnsAsync(users);

            var controller = new UserController(_serviceMock.Object);

            var result = await controller.GetAll();

            Assert.IsInstanceOf<OkObjectResult>(result);
            var ok = result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreSame(users, ok!.Value);
            _serviceMock.Verify(s => s.GetAllUsers(), Times.Once);
        }

        [Test]
        public async Task GetAll_WhenNoUsers_ReturnsOkWithEmptyList()
        {
            var users = new List<UserResponseDto>();
            _serviceMock.Setup(s => s.GetAllUsers()).ReturnsAsync(users);

            var controller = new UserController(_serviceMock.Object);

            var result = await controller.GetAll();

            Assert.IsInstanceOf<OkObjectResult>(result);
            var ok = result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.IsInstanceOf<List<UserResponseDto>>(ok!.Value);
            var list = ok.Value as List<UserResponseDto>;
            Assert.IsNotNull(list);
            Assert.AreEqual(0, list.Count);
            _serviceMock.Verify(s => s.GetAllUsers(), Times.Once);
        }
    }
}
