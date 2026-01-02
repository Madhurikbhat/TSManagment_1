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
    public class ProjectCodeServiceTests
    {
        private Mock<IMapper> _mapperMock = null!;

        [SetUp]
        public void Setup()
        {
            _mapperMock = new Mock<IMapper>();

            // Map ProjectCode -> ProjectCodeResponseDto
            _mapperMock.Setup(m => m.Map<ProjectCodeResponseDto>(It.IsAny<ProjectCode>()))
                .Returns((ProjectCode pc) => new ProjectCodeResponseDto
                {
                    Id = pc.Id,
                    Code = pc.Code,
                    ProjectName = pc.ProjectName,
                    ClientName = pc.ClientName,
                    IsBillable = pc.IsBillable,
                    IsActive = pc.IsActive
                });

            // Map CreateDto -> ProjectCode (used by CreateProject)
            _mapperMock.Setup(m => m.Map<ProjectCode>(It.IsAny<ProjectCodeCreateDto>()))
                .Returns((ProjectCodeCreateDto dto) => new ProjectCode
                {
                    Code = dto.Code,
                    ProjectName = dto.ProjectName,
                    ClientName = dto.ClientName,
                    IsBillable = dto.IsBillable,
                    IsActive = dto.IsActive
                });

            // Map CreateDto onto existing ProjectCode (used by UpdateProject)
            _mapperMock.Setup(m => m.Map(It.IsAny<ProjectCodeCreateDto>(), It.IsAny<ProjectCode>()))
                .Returns((ProjectCodeCreateDto src, ProjectCode dest) =>
                {
                    dest.Code = src.Code;
                    dest.ProjectName = src.ProjectName;
                    dest.ClientName = src.ClientName;
                    dest.IsBillable = src.IsBillable;
                    dest.IsActive = src.IsActive;
                    return dest;
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
        public async Task GetAllProjects_ReturnsMappedList()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var ctx = CreateContext(dbName);

            ctx.ProjectCodes.Add(new ProjectCode { Code = "C1", ProjectName = "P1", ClientName = "Cl1", IsActive = true, IsBillable = true });
            ctx.ProjectCodes.Add(new ProjectCode { Code = "C2", ProjectName = "P2", ClientName = "Cl2", IsActive = false, IsBillable = false });
            await ctx.SaveChangesAsync();

            var service = new ProjectCodeService(ctx, _mapperMock.Object);

            var result = await service.GetAllProjects();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Any(r => r.Code == "C1" && r.ProjectName == "P1"));
        }

        [Test]
        public async Task CreateProject_AddsEntityAndReturnsDto()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var ctx = CreateContext(dbName);

            var service = new ProjectCodeService(ctx, _mapperMock.Object);

            var dto = new ProjectCodeCreateDto
            {
                Code = "NEW",
                ProjectName = "NewProject",
                ClientName = "ClientX",
                IsBillable = true,
                IsActive = true
            };

            var returned = await service.CreateProject(dto);

            var saved = await ctx.ProjectCodes.FirstOrDefaultAsync(pc => pc.Code == "NEW");
            Assert.IsNotNull(saved);
            Assert.AreEqual(dto.ProjectName, saved.ProjectName);

            Assert.IsNotNull(returned);
            Assert.AreEqual(saved.Id, returned.Id);
            Assert.AreEqual(saved.Code, returned.Code);
        }

        [Test]
        public async Task UpdateProject_WhenExists_UpdatesAndReturnsDto()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var ctx = CreateContext(dbName);

            var existing = new ProjectCode { Code = "OLD", ProjectName = "Old", ClientName = "C", IsActive = true, IsBillable = false };
            ctx.ProjectCodes.Add(existing);
            await ctx.SaveChangesAsync();

            var service = new ProjectCodeService(ctx, _mapperMock.Object);

            var updateDto = new ProjectCodeCreateDto
            {
                Code = "UPDATED",
                ProjectName = "UpdatedName",
                ClientName = "ClientUpdated",
                IsBillable = true,
                IsActive = false
            };

            var returned = await service.UpdateProject(existing.Id, updateDto);

            Assert.IsNotNull(returned);
            Assert.AreEqual(existing.Id, returned.Id);
            Assert.AreEqual("UPDATED", returned.Code);

            var reloaded = await ctx.ProjectCodes.FindAsync(existing.Id);
            Assert.IsNotNull(reloaded);
            Assert.AreEqual("UPDATED", reloaded.Code);
            Assert.AreEqual("UpdatedName", reloaded.ProjectName);
        }

        [Test]
        public async Task UpdateProject_WhenNotFound_ReturnsNull()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var ctx = CreateContext(dbName);

            var service = new ProjectCodeService(ctx, _mapperMock.Object);

            var updateDto = new ProjectCodeCreateDto { Code = "X", ProjectName = "X" };

            var returned = await service.UpdateProject(9999, updateDto);

            Assert.IsNull(returned);
        }
    }
}
