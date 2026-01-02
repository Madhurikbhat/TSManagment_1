using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timesheet.Domain.Entities
{
    public class ProjectCode 
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string ProjectName { get; set; }
        public string ClientName { get; set; }
        public bool IsBillable { get; set; }
        public bool IsActive { get; set; }

        public ICollection<ProjectAssignment> ProjectAssignments { get; set; }
        public ICollection<TimesheetEntry> TimesheetEntries { get; set; }
    }
}
