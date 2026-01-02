using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Timesheet.Application.DTOs;
using Timesheet.Application.Interfaces;

namespace Timesheet.API.Controllers
{

    [ApiController]
    [Route("api/projectAssignments")]
    // [Authorize(Roles = "Manager")]
    [EnableCors("allowCors")]
    public class ProjectAssignmentController : ControllerBase
    {
        private readonly IProjectAssignmentService _service;

        public ProjectAssignmentController(IProjectAssignmentService service)
        {
            _service = service;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllProjectAssignment();
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProjectAssignmentCreateDto dto)
        {
            if (dto == null) return BadRequest();
            var created = await _service.CreateProjectAssignment(dto);
            // returns 201 with created resource in body; adjust to CreatedAtAction/GetById when you add a GET by id endpoint
            return CreatedAtAction(nameof(GetAll), null, created);
        }
        [HttpGet("user/{userId}/projects")]
        public async Task<IActionResult> GetProjectsByUser(int userId)
        {
            var projects = await _service.GetProjectsByUserId(userId);
            return Ok(projects);
        }
    }
}
