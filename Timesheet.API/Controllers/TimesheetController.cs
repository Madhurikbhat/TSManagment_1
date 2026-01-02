using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Timesheet.Application.DTOs;
using Timesheet.Application.DTOs.Timesheet;
using Timesheet.Application.Interfaces;
using Timesheet.Domain.Enums;

namespace Timesheet.API.Controllers
{
    [ApiController]
    [Route("api/timesheet")]
    // [Authorize(Roles = "Manager")]
    [EnableCors("allowCors")]
    public class TimesheetController : ControllerBase
    {
        private readonly ITimesheetService _service;
        public TimesheetController(ITimesheetService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllTimesheetEntries();
            return Ok(result);
        }

        [HttpGet("pendingTimesheet")]
        public async Task<IActionResult> GetTimesheets()
        {
            var result = await _service.GetTimesheetsByStatus(2);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTimesheetDto dto)
        {
            if (dto == null) return BadRequest();
            var created = await _service.CreateEntry(dto);
            return CreatedAtAction(nameof(GetAll), null, created);
        }

        [HttpPost("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] TimesheetStatusUpdateDto dto)
        {
            if (dto == null) return BadRequest();

            // require comment when rejecting
            if (dto.Status == (int)TimesheetStatus.Rejected && string.IsNullOrWhiteSpace(dto.Comment))
            {
                return BadRequest("Reject comment is required when status is Rejected.");
            }

            try
            {
                var updated = await _service.UpdateStatus(id, dto.Status, dto.Comment);
                if (updated == null) return NotFound();
                return Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/full-update")]
        public async Task<IActionResult> UpdateEntryFully(int id, [FromBody] TimesheetEntryDto dto)
        {
            if (dto == null) return BadRequest();

            // require comment when rejecting
            if (dto.Status == (int)TimesheetStatus.Rejected && string.IsNullOrWhiteSpace(dto.Comment))
            {
                return BadRequest("Reject comment is required when status is Rejected.");
            }

            try
            {
                var updated = await _service.UpdateEntryFully(id, dto);
                if (updated == null) return NotFound();
                return Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("user/{userId}/week")]
        public async Task<IActionResult> GetWeeklyEntriesForUser(int userId, [FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
        {
            var result = await _service.GetWeeklyEntriesForUser(userId, startDate, endDate);
            return Ok(result);
        }
    }
}