using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timesheet.Domain.Enums
{

    public enum UserRole
    {
        Employee = 1,
        Manager = 2
    }

    public enum TimesheetStatus
    {
        Draft = 1,
        Submitted = 2,
        Approved = 3,
        Rejected = 4
    }

}
