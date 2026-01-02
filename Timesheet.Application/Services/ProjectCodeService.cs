using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Timesheet.Application.DTOs;
using Timesheet.Application.Interfaces;
using Timesheet.Domain.Entities;
using Timesheet.Infrastructure;

namespace Timesheet.Application.Services
{
    public class ProjectCodeService : IProjectCodeService
    {
        private readonly TimesheetDbContext _context;
        private readonly IMapper _mapper;
        public ProjectCodeService(TimesheetDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<ProjectCodeResponseDto>> GetAllProjects()
        {
            var entities = await _context.ProjectCodes.ToListAsync();
            return entities.Select(p => _mapper.Map<ProjectCodeResponseDto>(p)).ToList();
        }
        public async Task<ProjectCodeResponseDto> CreateProject(ProjectCodeCreateDto dto)
        {
            var entity = _mapper.Map<ProjectCode>(dto);
            _context.ProjectCodes.Add(entity);
            await _context.SaveChangesAsync();
            return _mapper.Map<ProjectCodeResponseDto>(entity);
        }
        public async Task<ProjectCodeResponseDto?> UpdateProject(int id, ProjectCodeCreateDto dto)
        {
            var entity = await _context.ProjectCodes.FirstOrDefaultAsync(p => p.Id == id);
            if (entity == null) return null;

            // Map incoming DTO onto existing entity and persist
            _mapper.Map(dto, entity);
            await _context.SaveChangesAsync();

            return _mapper.Map<ProjectCodeResponseDto>(entity);
        }
    }
}
