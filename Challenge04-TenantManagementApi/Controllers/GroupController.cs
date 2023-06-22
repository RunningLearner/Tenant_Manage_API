using Microsoft.AspNetCore.Mvc;
using Challenge04_TenantManagementApi.Services;
using Challenge04_TenantManagementApi.Models;
using Microsoft.Graph.Models.ODataErrors;
using Challenge04_TenantManagementApi.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Challenge04_TenantManagementApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class GroupController : ControllerBase
{
    private readonly GroupService _service;
    private readonly ILogger<GroupController> _logger;

    public GroupController(GroupService service, ILogger<GroupController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // GET: api/Group/5
    [HttpGet("{id}")]
    [ExecutionTime]
    public async Task<ActionResult<GroupDto>> GetGroup([Required] string id)
    {
        return await _service.GetAsync(id);
    }

    // POST: api/Group
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    [ApiKeyAuth]
    [ExecutionTime]
    public async Task<ActionResult<GroupDto>> PostGroup([Required] CreateGroupDto groupDto)
    {
        var group = await _service.AddAsync(groupDto);
        return CreatedAtAction(nameof(GetGroup), new { id = group.Id }, group);
    }

    // PUT: api/Group/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    [ApiKeyAuth]
    [ExecutionTime]
    public async Task<IActionResult> PutGroup([Required] string id, [Required] GroupDto groupDto)
    {
        await _service.UpdateAsync(id, groupDto);
        return NoContent();
    }

    // DELETE: api/Group/5
    [HttpDelete("{id}")]
    [ApiKeyAuth]
    [ExecutionTime]
    public async Task<IActionResult> DeleteGroup([Required] string id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
