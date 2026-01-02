using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Domain.Enums;

namespace Timesheet.Application.DTOs
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Role { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; }
    }
}
