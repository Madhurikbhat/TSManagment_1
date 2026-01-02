using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timesheet.Application.DTOs
{
    public class ProjectCodeCreateDto
    {
        public string Code { get; set; }
        public string ProjectName { get; set; }
        public string ClientName { get; set; }
        public bool IsBillable { get; set; }
        public bool IsActive { get; set; }
    }
}
