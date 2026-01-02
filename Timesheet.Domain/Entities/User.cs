using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Domain.Enums;

namespace Timesheet.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public UserRole Role { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; }

        public ICollection<ProjectAssignment> ProjectAssignments { get; set; }
        public ICollection<TimesheetData> Timesheets { get; set; } 
    }
}
