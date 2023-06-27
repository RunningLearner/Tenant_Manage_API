using Challenge04_TenantManagementApi.Data;
using Challenge04_TenantManagementApi.Models;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using GrpahUser = Microsoft.Graph.Models.User;
using DbUser = Challenge04_TenantManagementApi.Models.User;
using Microsoft.EntityFrameworkCore;

namespace Challenge04_TenantManagementApi.Services;

public sealed class UserService
{
    private readonly ILogger<UserService> _logger;
    private readonly GraphServiceClient _graphClient;
    private readonly GraphDbContext _graphDbContext;

    public UserService(GraphServiceClient graphClient, ILogger<UserService> logger, GraphDbContext graphDbContext)
    {
        _logger = logger;
        _graphClient = graphClient;
        _graphDbContext = graphDbContext;
    }

    /// <summary>
    /// pageSize의 유저의 정보를 조회
    /// </summary>
    /// <param name="pageSize"></param>
    /// <param name="cursor"></param>
    /// <returns></returns>
    public async Task<(List<DbUser>, string?)> GetAllAsync(int pageSize = 10, string? cursor = null)
    {
        var query = _graphDbContext.Users.AsQueryable();

        if (cursor != null)
        {
            query = query.Where(user => user.Id.CompareTo(cursor) > 0);
        }

        var users = await query.OrderBy(user => user.Id).Take(pageSize + 1).ToListAsync();

        string? nextCursor = null;

        if (users.Count > pageSize)
        {
            nextCursor = users.Last().Id;
            users.RemoveAt(users.Count - 1);
        }

        return (users, nextCursor);
    }

    /// <summary>
    /// 특정 유저의 정보를 조회
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<UserDto> GetAsync(string id)
    {
        var user = await _graphDbContext.Users.FindAsync(id);

        if (user is null)
        {
            throw new KeyNotFoundException($"ID '{id}'를 가진 유저를 찾지 못했습니다.");
        }

        _logger.LogInformation("UserId : {Id}", user.Id);

        return ItemToDto(user);
    }

    /// <summary>
    /// 새로운 유저를 생성
    /// </summary>
    /// <param name="createUserDto"></param>
    /// <returns></returns>
    public async Task<UserDto> AddAsync(CreateUserDto createUserDto)
    {
        var user = new GrpahUser
        {
            AccountEnabled = true,
            DisplayName = createUserDto.DisplayName,
            MailNickname = createUserDto.MailNickname,
            UserPrincipalName = createUserDto.UserPrincipalName,
            PasswordProfile = new PasswordProfile
            {
                ForceChangePasswordNextSignIn = true,
                Password = Guid.NewGuid().ToString(),
            },
        };

        var createdUser = await _graphClient.Users.PostAsync(user);
        _logger.LogInformation("NewUser : {@NewUser}", createdUser);

        return ItemToDto(createdUser!);
    }

    /// <summary>
    /// 특정 유저의 정보를 수정
    /// </summary>
    /// <param name="id"></param>
    /// <param name="userDto"></param>
    /// <returns></returns>
    public async Task UpdateAsync(string id, UserDto userDto)
    {
        var user = new GrpahUser
        {
            DisplayName = userDto.DisplayName,
            MailNickname = userDto.MailNickname,
            UserPrincipalName = userDto.UserPrincipalName,
        };

        await _graphClient.Users[id].PatchAsync(user);
        _logger.LogInformation("UpdatedUser : {@User}", user);
    }

    /// <summary>
    /// 특정 유저를 삭제
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task DeleteAsync(string id)
    {
        var user = await _graphClient.Users[id].GetAsync();
        await _graphClient.Users[id].DeleteAsync();
        _logger.LogInformation("DeletedUser : {@DeletedUser}", user);
    }

    private static UserDto ItemToDto(GrpahUser user)
    {
        return new UserDto
        {
            Id = user.Id!,
            DisplayName = user.DisplayName,
            UserPrincipalName = user.UserPrincipalName,
            MailNickname = user.MailNickname
        };
    }

    private static UserDto ItemToDto(DbUser user)
    {
        return new UserDto
        {
            Id = user.Id!,
            DisplayName = user.DisplayName,
            UserPrincipalName = user.UserPrincipalName,
            MailNickname = user.MailNickname
        };
    }
}