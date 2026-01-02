using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Application.DTOs;
using Timesheet.Application.Interfaces;
using Timesheet.Domain.Entities;
using Timesheet.Infrastructure;

namespace Timesheet.Application.Services
{
    public class ProjectAssignmentService : IProjectAssignmentService
    {
        private readonly TimesheetDbContext _context;
        private readonly IMapper _mapper;
        public ProjectAssignmentService(TimesheetDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<ProjectAssignmentResponseDto> CreateProjectAssignment(ProjectAssignmentCreateDto dto)
        {
            // check if an assignment for the same user and project already exists
            var existing = await _context.ProjectAssignments
                .AsNoTracking()
                .FirstOrDefaultAsync(pa => pa.UserId == dto.UserId && pa.ProjectCodeId == dto.ProjectCodeId);

            if (existing != null)
            {
                throw new Exception(
                    $"Project already assigned to user (UserId={dto.UserId}, ProjectCodeId={dto.ProjectCodeId})");
            }

            var entity = _mapper.Map<ProjectAssignment>(dto);
            _context.ProjectAssignments.Add(entity);
            await _context.SaveChangesAsync();
            return _mapper.Map<ProjectAssignmentResponseDto>(entity);
        }

        public async Task<List<ProjectAssignmentResponseDto>> GetAllProjectAssignment()
        {
            var entities = await _context.ProjectAssignments.ToListAsync();
            return entities.Select(p => _mapper.Map<ProjectAssignmentResponseDto>(p)).ToList();
        }
        public async Task<List<ProjectCodeResponseDto>> GetProjectsByUserId(int userId)
        {
            var projects = await _context.ProjectAssignments
                .Where(pa => pa.UserId == userId)
                .Include(pa => pa.ProjectCode)
                .Select(pa => pa.ProjectCode)
                .Where(pc => pc.IsActive)
                .ToListAsync();

            var distinct = projects.GroupBy(pc => pc.Id).Select(g => g.First()).ToList();
            return distinct.Select(pc => _mapper.Map<ProjectCodeResponseDto>(pc)).ToList();
        }
    }
}
