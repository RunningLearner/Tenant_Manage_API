using Microsoft.AspNetCore.Mvc;
using Challenge04_TenantManagementApi.Services;
using Challenge04_TenantManagementApi.Models;
using Microsoft.Graph.Models.ODataErrors;
using Challenge04_TenantManagementApi.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Challenge04_TenantManagementApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class UserController : ControllerBase
{
    private readonly UserService _service;
    private readonly ILogger<UserController> _logger;

    public UserController(UserService service, ILogger<UserController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // GET: api/User/5
    [HttpGet("{id}")]
    [ExecutionTime]
    public async Task<ActionResult<UserDto>> GetUser([Required] string id)
    {
        return await _service.GetAsync(id);
    }

    // POST: api/User
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    [ApiKeyAuth]
    [ExecutionTime]
    public async Task<ActionResult<UserDto>> PostUser([Required] CreateUserDto userDto)
    {
        var user = await _service.AddAsync(userDto);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    // PUT: api/User/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    [ApiKeyAuth]
    [ExecutionTime]
    public async Task<IActionResult> PutUser([Required] string id, [Required] UserDto userDto)
    {
        await _service.UpdateAsync(id, userDto);
        return NoContent();
    }

    // DELETE: api/User/5
    [HttpDelete("{id}")]
    [ApiKeyAuth]
    [ExecutionTime]
    public async Task<IActionResult> DeleteUser([Required] string id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
