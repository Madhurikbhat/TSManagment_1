using System;
using System.Collections.Generic;
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
    public class ProjectAssignmentServiceTests
    {
        private Mock<IMapper> _mapperMock = null!;

        [SetUp]
        public void Setup()
        {
            _mapperMock = new Mock<IMapper>();

            // Map CreateDto -> Entity
            _mapperMock.Setup(m => m.Map<ProjectAssignment>(It.IsAny<ProjectAssignmentCreateDto>()))
                .Returns((ProjectAssignmentCreateDto dto) => new ProjectAssignment
                {
                    UserId = dto.UserId,
                    ProjectCodeId = dto.ProjectCodeId,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate
                });

            // Map Entity -> ResponseDto
            _mapperMock.Setup(m => m.Map<ProjectAssignmentResponseDto>(It.IsAny<ProjectAssignment>()))
                .Returns((ProjectAssignment ent) => new ProjectAssignmentResponseDto
                {
                    Id = ent.Id,
                    UserId = ent.UserId,
                    ProjectCodeId = ent.ProjectCodeId,
                    StartDate = ent.StartDate,
                    EndDate = ent.EndDate
                });

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
        }

        private TimesheetDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<TimesheetDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new TimesheetDbContext(options);
        }

        [Test]
        public async Task CreateProjectAssignment_AddsEntityAndReturnsResponseDto()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var ctx = CreateContext(dbName);

            var service = new ProjectAssignmentService(ctx, _mapperMock.Object);

            var createDto = new ProjectAssignmentCreateDto
            {
                UserId = 5,
                ProjectCodeId = 10,
                StartDate = new DateTime(2025,1,1),
                EndDate = new DateTime(2025,12,31)
            };

            var returned = await service.CreateProjectAssignment(createDto);

            // verify persisted
            var saved = await ctx.ProjectAssignments.FirstOrDefaultAsync();
            Assert.IsNotNull(saved);
            Assert.AreEqual(createDto.UserId, saved.UserId);
            Assert.AreEqual(createDto.ProjectCodeId, saved.ProjectCodeId);

            // verify returned dto reflects saved entity
            Assert.IsNotNull(returned);
            Assert.AreEqual(saved.Id, returned.Id);
            Assert.AreEqual(saved.UserId, returned.UserId);
        }

        [Test]
        public async Task GetAllProjectAssignment_ReturnsMappedList()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var ctx = CreateContext(dbName);

            ctx.ProjectAssignments.Add(new ProjectAssignment { UserId = 1, ProjectCodeId = 2, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1) });
            ctx.ProjectAssignments.Add(new ProjectAssignment { UserId = 2, ProjectCodeId = 3, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1) });
            await ctx.SaveChangesAsync();

            var service = new ProjectAssignmentService(ctx, _mapperMock.Object);

            var result = await service.GetAllProjectAssignment();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(r => r.UserId > 0));
        }

        [Test]
        public async Task GetProjectsByUserId_ReturnsDistinctActiveProjects()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var ctx = CreateContext(dbName);

            var project = new ProjectCode { Id = 100, Code = "P100", ProjectName = "Proj100", ClientName = "C", IsActive = true, IsBillable = true };
            var inactiveProject = new ProjectCode { Id = 200, Code = "P200", ProjectName = "Proj200", ClientName = "C2", IsActive = false, IsBillable = false };

            ctx.ProjectCodes.AddRange(project, inactiveProject);

            // two assignments to same project for same user to test distinct
            ctx.ProjectAssignments.Add(new ProjectAssignment { UserId = 77, ProjectCode = project, ProjectCodeId = project.Id, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1) });
            ctx.ProjectAssignments.Add(new ProjectAssignment { UserId = 77, ProjectCode = project, ProjectCodeId = project.Id, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(2) });

            // assignment to inactive project should be ignored
            ctx.ProjectAssignments.Add(new ProjectAssignment { UserId = 77, ProjectCode = inactiveProject, ProjectCodeId = inactiveProject.Id, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(2) });

            await ctx.SaveChangesAsync();

            var service = new ProjectAssignmentService(ctx, _mapperMock.Object);

            var projects = await service.GetProjectsByUserId(77);

            Assert.IsNotNull(projects);
            Assert.AreEqual(1, projects.Count);
            Assert.AreEqual(project.Id, projects[0].Id);
            Assert.IsTrue(projects[0].IsActive);
        }
    }
}
