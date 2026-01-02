using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Application.DTOs;

namespace Timesheet.Application.Interfaces
{
    public interface IUserService
    {
        public Task<List<UserResponseDto>> GetAllUsers();
    }
}
