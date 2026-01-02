using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Timesheet.Application.DTOs;
using Timesheet.Application.Interfaces;

namespace Timesheet.API.Controllers
{
    [ApiController]
    [Route("api/project-codes")]
    [EnableCors("allowCors")]
    public class ProjectCodeController : ControllerBase
    {
        private readonly IProjectCodeService _service;

        public ProjectCodeController(IProjectCodeService service)
        {
            _service = service;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllProjects();
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProjectCodeCreateDto dto)
        {
            if (dto == null) return BadRequest();
            var created = await _service.CreateProject(dto);
            return CreatedAtAction(nameof(GetAll), null, created);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProjectCodeCreateDto dto)
        {
            if (dto == null) return BadRequest();

            var updated = await _service.UpdateProject(id, dto);
            if (updated == null) return NotFound();

            return Ok(updated);
        }

    }
}
