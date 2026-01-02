using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Application.DTOs;

namespace Timesheet.Application.Interfaces
{
    public interface IProjectAssignmentService
    {
        public Task<List<ProjectAssignmentResponseDto>> GetAllProjectAssignment();
        public Task<ProjectAssignmentResponseDto> CreateProjectAssignment(ProjectAssignmentCreateDto dto);
        Task<List<ProjectCodeResponseDto>> GetProjectsByUserId(int userId);
    }
}
