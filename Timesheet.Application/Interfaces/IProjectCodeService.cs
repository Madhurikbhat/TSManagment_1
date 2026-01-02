using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Application.DTOs;

namespace Timesheet.Application.Interfaces
{
    public interface  IProjectCodeService
    {
        public Task<List<ProjectCodeResponseDto>> GetAllProjects() ;
        public Task<ProjectCodeResponseDto> CreateProject(ProjectCodeCreateDto dto);
        Task<ProjectCodeResponseDto?> UpdateProject(int id, ProjectCodeCreateDto dto);

    }
}
