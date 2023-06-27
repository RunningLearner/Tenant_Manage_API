using Challenge04_TenantManagementApi.Models;
using Microsoft.Graph;
using GraphGroup = Microsoft.Graph.Models.Group;
using DbGroup = Challenge04_TenantManagementApi.Models.Group;
using Challenge04_TenantManagementApi.Data;
using Microsoft.EntityFrameworkCore;

namespace Challenge04_TenantManagementApi.Services;

public sealed class GroupService
{
    private readonly ILogger<GroupService> _logger;
    private readonly GraphServiceClient _graphClient;
    private readonly GraphDbContext _graphDbContext;

    public GroupService(GraphServiceClient graphClient, ILogger<GroupService> logger, GraphDbContext graphDbContext)
    {
        _logger = logger;
        _graphClient = graphClient;
        _graphDbContext = graphDbContext;
    }

    /// <summary>
    /// pageSize 만큼 그룹의 정보를 조회
    /// </summary>
    /// <param name="pageSize">가져올 정보의 수량</param>
    /// <param name="cursor">시작할 위치를 가리키는 커서</param>
    /// <returns>그룹의 리스트, 다음 리스트의 시작을 가리키는 커서</returns>
    public async Task<(List<DbGroup>, DateTimeOffset?)> GetAllAsync(int pageSize = 10, DateTimeOffset? cursor = null)
    {
        var query = _graphDbContext.Groups.AsQueryable();

        if (cursor != null)
        {
            query = query.Where(group => group.CreatedDateTime > cursor);
        }

        var groups = await query.OrderBy(group => group.CreatedDateTime).Take(pageSize + 1).ToListAsync();

        DateTimeOffset? nextCursor = null;

        if (groups.Count > pageSize)
        {
            nextCursor = groups.Last().CreatedDateTime;
            groups.RemoveAt(groups.Count - 1);
        }

        return (groups, nextCursor);
    }

    /// <summary>
    /// 특정 그룹의 정보를 조회 
    /// </summary>
    /// <param name="id">조회할 그룹의 ID</param>
    /// <returns>조회된 그룹의 정보</returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<GroupDto> GetAsync(string id)
    {
        var group = await _graphDbContext.Groups.FindAsync(id);

        if (group is null)
        {
            throw new KeyNotFoundException($"ID '{id}'를 가진 그룹을 찾지 못했습니다.");
        }

        _logger.LogInformation("GroupId : {Id}", group.Id);

        return ItemToDto(group);
    }

    /// <summary>
    /// 새로운 그룹을 생성
    /// </summary>
    /// <param name="createGroupDto">그룹 생성에 사용될 정보</param>
    /// <returns>생성된 그룹의 정보</returns>
    public async Task<GroupDto> AddAsync(CreateGroupDto createGroupDto)
    {
        var group = new GraphGroup
        {
            Description = createGroupDto.Description,
            DisplayName = createGroupDto.DisplayName,
            MailNickname = createGroupDto.MailNickname,
            MailEnabled = true,
            GroupTypes = new List<string> { "Unified" },
            SecurityEnabled = false,
        };

        var createdGroup = await _graphClient.Groups.PostAsync(group);
        _logger.LogInformation("NewGroup : {@NewGroup}", createdGroup);

        return ItemToDto(createdGroup!);
    }

    /// <summary>
    /// 특정 그룹의 정보를 수정
    /// </summary>
    /// <param name="id">수정하려는 그룹의 ID</param>
    /// <param name="groupDto">수정에 사용될 정보</param>
    /// <returns></returns>
    public async Task UpdateAsync(string id, GroupDto groupDto)
    {
        var group = new GraphGroup
        {
            DisplayName = groupDto.DisplayName,
            MailNickname = groupDto.MailNickname,
            Description = groupDto.Description,
        };

        await _graphClient.Groups[id].PatchAsync(group);
        _logger.LogInformation("UpdatedGroup : {@Group}", group);
    }

    /// <summary>
    /// 특정 그룹을 삭제
    /// </summary>
    /// <param name="id">삭제하고자 하는 그룹의 ID</param>
    /// <returns></returns>
    public async Task DeleteAsync(string id)
    {
        var group = await _graphClient.Groups[id].GetAsync();
        await _graphClient.Groups[id].DeleteAsync();
        _logger.LogInformation("DeletedGroup : {@DeletedGroup}", group);
    }

    private static GroupDto ItemToDto(GraphGroup group)
    {
        return new GroupDto
        {
            Id = group.Id!,
            DisplayName = group.DisplayName,
            Description = group.Description,
            MailNickname = group.MailNickname
        };
    }

    private static GroupDto ItemToDto(DbGroup group)
    {
        return new GroupDto
        {
            Id = group.Id!,
            DisplayName = group.DisplayName,
            Description = group.Description,
            MailNickname = group.MailNickname,
            CreatedDateTime = group.CreatedDateTime
        };
    }
}