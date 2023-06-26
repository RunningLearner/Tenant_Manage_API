using Microsoft.AspNetCore.Mvc;
using Challenge04_TenantManagementApi.Services;
using Challenge04_TenantManagementApi.Models;
using Challenge04_TenantManagementApi.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Challenge04_TenantManagementApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class GroupController : ControllerBase
{
    private readonly GroupService _service;
    private readonly ILogger<GroupController> _logger;
    private readonly IUrlHelper _urlHelper;

    public GroupController(GroupService service, ILogger<GroupController> logger, IUrlHelper urlHelper)
    {
        _service = service;
        _logger = logger;
        _urlHelper = urlHelper;
    }

    // GET: api/User
    [HttpGet(Name = "GetAllGroups")]
    [ExecutionTime]
    public async Task<ActionResult<PageResponse<Group>>> GetAllGroup([FromQuery] GetAllDto getAllDto)
    {
        string? cursor = null;

        if (!string.IsNullOrEmpty(getAllDto.NextUrl))
        {
            var uri = new Uri(getAllDto.NextUrl);
            var queryParameters = HttpUtility.ParseQueryString(uri.Query);
            cursor = queryParameters.Get("cursor");
        }

        var (groups, nextCursor) = await _service.GetAllAsync(getAllDto.PageSize, cursor);
        var response = new PageResponse<Group>
        {
            Data = groups
        };

        if (nextCursor != null)
        {
            var urlParams = new { getAllDto.PageSize, cursor = nextCursor };
            response.NextUrl = _urlHelper.Link("GetAllGroups", urlParams);
        }

        return Ok(response);
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
