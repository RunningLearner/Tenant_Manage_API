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
    private readonly IUrlHelper _urlHelper;

    public GroupController(GroupService service, IUrlHelper urlHelper)
    {
        _service = service;
        _urlHelper = urlHelper;
    }

    /// <summary>
    /// GET: api/User
    /// </summary>
    /// <param name="getAllDto"></param>
    /// <returns></returns>
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

    /// <summary>
    /// GET: api/Group/5
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [ExecutionTime]
    public async Task<ActionResult<GroupDto>> GetGroup([Required] string id)
    {
        return await _service.GetAsync(id);
    }

    /// <summary>
    /// POST: api/Group
    /// </summary>
    /// <param name="groupDto"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiKeyAuth]
    [ExecutionTime]
    public async Task<ActionResult<GroupDto>> PostGroup([Required] CreateGroupDto groupDto)
    {
        var group = await _service.AddAsync(groupDto);
        return CreatedAtAction(nameof(GetGroup), new { id = group.Id }, group);
    }

    /// <summary>
    /// PUT: api/Group/5
    /// </summary>
    /// <param name="id"></param>
    /// <param name="groupDto"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [ApiKeyAuth]
    [ExecutionTime]
    public async Task<IActionResult> PutGroup([Required] string id, [Required] GroupDto groupDto)
    {
        await _service.UpdateAsync(id, groupDto);
        return NoContent();
    }

    /// <summary>
    /// DELETE: api/Group/5
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [ApiKeyAuth]
    [ExecutionTime]
    public async Task<IActionResult> DeleteGroup([Required] string id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
