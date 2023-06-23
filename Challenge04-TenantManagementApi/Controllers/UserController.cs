using Microsoft.AspNetCore.Mvc;
using Challenge04_TenantManagementApi.Services;
using Challenge04_TenantManagementApi.Models;
using Challenge04_TenantManagementApi.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Challenge04_TenantManagementApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class UserController : ControllerBase
{
    private readonly UserService _service;
    private readonly ILogger<UserController> _logger;
    private readonly IUrlHelper _urlHelper;

    public UserController(UserService service, ILogger<UserController> logger, IUrlHelper urlHelper)
    {
        _service = service;
        _logger = logger;
        _urlHelper = urlHelper;
    }

    // GET: api/User/5
    [HttpGet(Name = "GetAllUsers")]
    [ExecutionTime]
    public async Task<ActionResult<PageResponse<User>>> GetAllUser([FromQuery] GetAllUserDto getAllUserDto)
    {
        string? cursor = null;

        if (!string.IsNullOrEmpty(getAllUserDto.NextUrl))
        {
            var uri = new Uri(getAllUserDto.NextUrl);
            var queryParameters = HttpUtility.ParseQueryString(uri.Query);
            cursor = queryParameters.Get("cursor");
        }

        var (users, nextCursor) = await _service.GetAllAsync(getAllUserDto.PageSize, cursor);
        var response = new PageResponse<User>
        {
            Data = users
        };

        if (nextCursor != null)
        {
            var urlParams = new { getAllUserDto.PageSize, cursor = nextCursor };
            response.NextUrl = _urlHelper.Link("GetAllUsers", urlParams);
        }

        return Ok(response);
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
