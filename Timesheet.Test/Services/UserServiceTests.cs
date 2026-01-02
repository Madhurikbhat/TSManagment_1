using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Timesheet.Application.DTOs;
using Timesheet.Application.Services;
using Timesheet.Domain.Entities;
using Timesheet.Infrastructure;

namespace Timesheet.Test.Services
{
    public class UserServiceTests
    {
        private Mock<IMapper> _mapperMock = null!;

        [SetUp]
        public void Setup()
        {
            _mapperMock = new Mock<IMapper>();

            _mapperMock.Setup(m => m.Map<UserResponseDto>(It.IsAny<User>()))
                .Returns((User u) => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Role = (int)u.Role,
                    Email = u.Email,
                    Password = u.Password,
                    IsActive = u.IsActive
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
        public async Task GetAllUsers_ReturnsMappedList()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var ctx = CreateContext(dbName);

            ctx.Users.Add(new User { Id = 1, Name = "Alice", Role = Domain.Enums.UserRole.Employee, Email = "Alice@gmail.com", Password ="alice", IsActive = true });
            ctx.Users.Add(new User { Id = 2, Name = "Bob", Role = Domain.Enums.UserRole.Manager, Email = "Bob@gmail.com", Password = "bob", IsActive = true });
            await ctx.SaveChangesAsync();

            var svc = new UserService(ctx, _mapperMock.Object);

            var result = await svc.GetAllUsers();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Any(u => u.Name == "Alice" && u.Role == (int)Domain.Enums.UserRole.Employee));
            Assert.IsTrue(result.Any(u => u.Name == "Bob" && u.Role == (int)Domain.Enums.UserRole.Manager));
        }

        [Test]
        public async Task GetAllUsers_WhenNoUsers_ReturnsEmptyList()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var ctx = CreateContext(dbName);

            var svc = new UserService(ctx, _mapperMock.Object);

            var result = await svc.GetAllUsers();

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }
    }
}
