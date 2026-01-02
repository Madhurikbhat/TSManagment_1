using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Timesheet.Application.DTOs;
using Timesheet.Application.Interfaces;

namespace Timesheet.API.Controllers
{
    [ApiController]
    [Route("api/user")]
    // [Authorize(Roles = "Manager")]
    [EnableCors("allowCors")]
    public class UserController :ControllerBase
    {
        private readonly IUserService _service;

        public UserController(IUserService service)
        {
            _service = service;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllUsers();
            return Ok(result);
        }
    }
}
