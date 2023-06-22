using Challenge04_TenantManagementApi.Models;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace Challenge04_TenantManagementApi.Services;

public sealed class GroupService
{
    private readonly ILogger<GroupService> _logger;
    private readonly GraphServiceClient _graphClient;

    public GroupService(GraphServiceClient graphClient, ILogger<GroupService> logger)
    {
        _logger = logger;
        _graphClient = graphClient;
    }

    public async Task<GroupDto> GetAsync(string id)
    {
        var group = await _graphClient.Groups[id].GetAsync();
        _logger.LogInformation("GroupId : {Id}", group?.Id);

        return ItemToDto(group!);
    }

    public async Task<GroupDto> AddAsync(CreateGroupDto createGroupDto)
    {
        var group = new Group
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
        var group = new Group
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

    private static GroupDto ItemToDto(Group group)
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