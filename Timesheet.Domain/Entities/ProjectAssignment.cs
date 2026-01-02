using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timesheet.Domain.Entities
{
    public class ProjectAssignment 
    {
        public int Id { get; set; }

        // FK to User (Employee)
        public int UserId { get; set; }
        public User User { get; set; }

        // FK to ProjectCode
        public int ProjectCodeId { get; set; }
        public ProjectCode ProjectCode { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
