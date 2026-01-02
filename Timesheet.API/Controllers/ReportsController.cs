using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Timesheet.Application.Interfaces;
using Timesheet.Application.Services;

namespace Timesheet.API.Controllers
{
    [ApiController]
    [Route("api/report")]
    // [Authorize(Roles = "Manager")]
    [EnableCors("allowCors")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _service;

        public ReportsController(IReportService service)
        {
            _service = service;
        }
        [HttpGet("employee-hours")]
        public async Task<IActionResult> GetEmployeeHours([FromQuery] DateOnly? startDate = null, [FromQuery] DateOnly? endDate = null)
        {
            var result = await _service.GetEmployeeHoursSummary(startDate, endDate);
            return Ok(result);
        }
        [HttpGet("project-hours")]
        public async Task<IActionResult> GetProjectHours([FromQuery] DateOnly? startDate = null, [FromQuery] DateOnly? endDate = null)
        {
            var result = await _service.GetProjectHoursSummary(startDate, endDate);
            return Ok(result);
        }
        [HttpGet("billable-hours")]
        public async Task<IActionResult> GetBillableHours([FromQuery] DateOnly? startDate = null, [FromQuery] DateOnly? endDate = null)
        {
            var result = await _service.GetBillableHoursSummary(startDate, endDate);
            return Ok(result);
        }
    }
}
