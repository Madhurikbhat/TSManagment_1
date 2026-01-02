using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Timesheet.Application.DTOs;
using Timesheet.Application.Interfaces;
using Timesheet.Infrastructure;

namespace Timesheet.Application.Services
{
    public class UserService :IUserService
    {
        private readonly TimesheetDbContext _context;
        private readonly IMapper _mapper;
        public UserService(TimesheetDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<UserResponseDto>> GetAllUsers()
        {
            var entities = await _context.Users.ToListAsync();
            return entities.Select(p => _mapper.Map<UserResponseDto>(p)).ToList();
        }
    }
}
