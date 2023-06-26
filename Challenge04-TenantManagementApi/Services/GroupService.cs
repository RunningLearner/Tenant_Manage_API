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

    public async Task<(List<DbGroup>, string?)> GetAllAsync(int pageSize = 10, string? cursor = null)
    {
        var query = _graphDbContext.Groups.AsQueryable();

        if (cursor != null)
        {
            query = query.Where(group => group.Id.CompareTo(cursor) > 0);
        }

        var groups = await query.OrderBy(group => group.Id).Take(pageSize + 1).ToListAsync();

        string? nextCursor = null;
        if (groups.Count > pageSize)
        {
            nextCursor = groups.Last().Id;
            groups.RemoveAt(groups.Count - 1);
        }

        return (groups, nextCursor);
    }

    public async Task<GroupDto> GetAsync(string id)
    {
        var group = await _graphClient.Groups[id].GetAsync();
        _logger.LogInformation("GroupId : {Id}", group?.Id);

        return ItemToDto(group!);
    }

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
}