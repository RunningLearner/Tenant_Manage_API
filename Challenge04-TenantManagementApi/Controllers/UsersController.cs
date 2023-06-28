using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Extensions;
using Challenge04_TenantManagementApi.Services;
using Challenge04_TenantManagementApi.Models;
using Challenge04_TenantManagementApi.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Challenge04_TenantManagementApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class UsersController : ControllerBase
{
    private readonly UserService _service;
    private readonly IUrlHelper _urlHelper;
    private readonly IHttpContextAccessor _accessor;
    public UsersController(UserService service, IUrlHelper urlHelper, IHttpContextAccessor accessor)
    {
        _service = service;
        _urlHelper = urlHelper;
        _accessor = accessor;
    }

    /// <summary>
    /// GET: api/User
    /// 유저들의 정보를 요청받은 수량만큼 조회
    /// </summary>
    /// <param name="getAllDto">조회 수량과 조회 시작점을 가리키는 URL</param>
    /// <response code="200">유저들의 정보 목록과 다음 시작점을 가리키는 URL</response>
    [HttpGet(Name = "GetAllUsers")]
    [ExecutionTime]
    public async Task<ActionResult<PageResponse<User>>> GetAllUser([Range(10, 50)] int pageSize = 10)
    {
        var context = _accessor.HttpContext;
        DateTimeOffset? cursor = null;

        if (context is not null)
        {
            cursor = GetDateTimeStringFromUrl(context);
        }

        var (users, nextCursor) = await _service.GetAllAsync(pageSize, cursor);
        var response = new PageResponse<User>
        {
            Data = users
        };

        if (nextCursor != null)
        {
            var localizedTimeString = nextCursor.Value.ToString("O");
            var urlParams = new { pageSize, nextCursor = localizedTimeString };
            response.NextUrl = _urlHelper.Link("GetAllUsers", urlParams);
        }

        return Ok(response);
    }

    /// <summary>
    /// GET: api/User/5
    /// 특정 유저의 정보를 조회
    /// </summary>
    /// <param name="id">조회할 유저의 ID</param>
    /// <response code="200">특정 유저의 조회에 성공할 경우</response>
    [HttpGet("{id}")]
    [ExecutionTime]
    public async Task<ActionResult<UserDto>> GetUser([Required] string id)
    {
        return await _service.GetAsync(id);
    }

    /// <summary>
    /// POST: api/User
    /// 새로운 유저를 생성
    /// </summary>
    /// <param name="userDto">생성할 유저의 정보</param>
    /// <response code="201">생성에 성공할 경우</response>
    [HttpPost]
    [ApiKeyAuth]
    [ExecutionTime]
    public async Task<ActionResult<UserDto>> PostUser([Required] CreateUserDto userDto)
    {
        var user = await _service.AddAsync(userDto);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    /// <summary>
    /// PUT: api/User/5
    /// 유저의 정보를 수정
    /// </summary>
    /// <param name="id">수정할 유저의 ID</param>
    /// <param name="userDto">수정할 정보</param>
    /// <response code="204">수정이 성공할 경우</response>
    [HttpPut("{id}")]
    [ApiKeyAuth]
    [ExecutionTime]
    public async Task<IActionResult> PutUser([Required] string id, [Required] UserDto userDto)
    {
        await _service.UpdateAsync(id, userDto);
        return NoContent();
    }

    /// <summary>
    /// DELETE: api/User/5
    /// 특정 유저를 삭제
    /// </summary>
    /// <param name="id">삭제할 유저의 ID</param>
    /// <response code="204">삭제가 성공할 경우</response>
    [HttpDelete("{id}")]
    [ApiKeyAuth]
    [ExecutionTime]
    public async Task<IActionResult> DeleteUser([Required] string id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    private static DateTimeOffset? GetDateTimeStringFromUrl(HttpContext context)
    {
        var uri = new Uri(context.Request.GetDisplayUrl());
        var queryParameters = HttpUtility.ParseQueryString(uri.Query);
        var cursor = queryParameters.Get("nextCursor");

        if (cursor == null)
        {
            return null;
        }

        if (!DateTimeOffset.TryParse(cursor, out DateTimeOffset parsed))
        {
            throw new ArgumentException($"'{cursor}'은 DateTimeOffset 형식이 아닙니다.");
        }

        return parsed;
    }
}
