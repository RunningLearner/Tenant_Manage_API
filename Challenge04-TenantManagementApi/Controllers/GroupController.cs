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
    /// 그룹들의 정보를 요청받은 수량만큼 조회
    /// </summary>
    /// <param name="getAllDto">조회 수량과 조회 시작점을 가리키는 URL</param>
    /// <response code="200">그룹들의 정보 목록과 다음 시작점을 가리키는 URL</response>
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
    /// 특정 유저의 정보를 조회
    /// </summary>
    /// <param name="id">조회할 그룹의 ID</param>
    /// <response code="200">특정 그룹의 조회에 성공할 경우</response>
    [HttpGet("{id}")]
    [ExecutionTime]
    public async Task<ActionResult<GroupDto>> GetGroup([Required] string id)
    {
        return await _service.GetAsync(id);
    }

    /// <summary>
    /// POST: api/Group
    /// 새로운 그룹을 생성
    /// </summary>
    /// <param name="groupDto">생성할 그룹의 정보</param>
    /// <response code="201">생성에 성공할 경우</response>
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
    /// 특정 그룹의 정보를 수정
    /// </summary>
    /// <param name="id">수정할 그룹의 ID</param>
    /// <param name="groupDto">수정에 사용될 정보</param>
    /// <response code="204">수정에 성공할 경우</response>
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
    /// 특정 그룹을 삭제
    /// </summary>
    /// <param name="id">삭제할 그룹의 ID</param>
    /// <response code="204">삭제에 성공할 경우</response>
    [HttpDelete("{id}")]
    [ApiKeyAuth]
    [ExecutionTime]
    public async Task<IActionResult> DeleteGroup([Required] string id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
