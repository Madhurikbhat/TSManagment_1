using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Domain.Entities;

namespace Timesheet.Application.DTOs
{
    public class ProjectAssignmentResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProjectCodeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
