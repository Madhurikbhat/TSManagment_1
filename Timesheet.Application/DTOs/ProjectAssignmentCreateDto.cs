namespace Timesheet.Application.DTOs
{
    public class ProjectAssignmentCreateDto
    {

        public int UserId { get; set; }
        public int ProjectCodeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
