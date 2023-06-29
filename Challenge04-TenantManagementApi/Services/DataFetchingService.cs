using Challenge04_TenantManagementApi.Data;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using DbUser = Challenge04_TenantManagementApi.Models.User;
using GraphUser = Microsoft.Graph.Models.User;
using DbGroup = Challenge04_TenantManagementApi.Models.Group;
using GraphGroup = Microsoft.Graph.Models.Group;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;

namespace Challenge04_TenantManagementApi.Services;

public sealed class DataFetchingService
{
    private readonly GraphServiceClient _graphClient;
    private readonly ILogger<DataFetchingService> _logger;
    private const int RetryCount = 5;

    public DataFetchingService(GraphServiceClient graphClient, ILogger<DataFetchingService> logger)
    {
        _graphClient = graphClient;
        _logger = logger;
    }

    /// <summary>
    /// 그룹과 유저의 데이터를 graph에서 가져와 db에 저장
    /// </summary>
    /// <param name="dbContext"></param>
    public async Task FetchData(GraphDbContext dbContext)
    {
        try
        {
            await FetchUserData(dbContext);
            await FetchGroupData(dbContext);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "데이터를 가져오는 중 에러 발생");
        }
    }

    private async Task FetchUserData(GraphDbContext dbContext)
    {
        var retryHandlerOption = new RetryHandlerOption
        {
            MaxRetry = RetryCount,
        };

        var usersResponse = await _graphClient.Users.GetAsync(requestConfiguration =>
        {
            requestConfiguration.QueryParameters.Select = new[] { "id", "displayName", "userPrincipalName", "mailNickname", "createdDateTime" };
            requestConfiguration.Options.Add(retryHandlerOption);
        }) ?? throw new ServiceException("GraphClient가 응답하지 않습니다.");

        List<DbUser> userList = new();
        var pageIterator = GetUserPageIterator(userList, usersResponse);
        await pageIterator.IterateAsync();
        await StoreUserToDb(dbContext, userList);
    }

    private async Task FetchGroupData(GraphDbContext dbContext)
    {
        var retryHandlerOption = new RetryHandlerOption
        {
            MaxRetry = RetryCount,
        };

        var groupsResponse = await _graphClient.Groups.GetAsync(requestConfiguration =>
        {
            requestConfiguration.QueryParameters.Select = new[] { "id", "displayName", "description", "mailNickname", "createdDateTime" };
            requestConfiguration.Options.Add(retryHandlerOption);
        }) ?? throw new ServiceException("GraphClient가 응답하지 않습니다.");

        List<DbGroup> groupList = new();
        var pageIterator = GetGroupPageIterator(groupList, groupsResponse);
        await pageIterator.IterateAsync();
        await StoreGroupToDb(dbContext, groupList);
    }

    private PageIterator<GraphGroup, GroupCollectionResponse> GetGroupPageIterator(List<DbGroup> groupList, GroupCollectionResponse groupsResponse)
    {
        return PageIterator<GraphGroup, GroupCollectionResponse>
            .CreatePageIterator(_graphClient, groupsResponse, (group) =>
            {
                var dbGroup = new DbGroup
                {
                    Id = group.Id!,
                    DisplayName = group.DisplayName,
                    Description = group.Description,
                    MailNickname = group.MailNickname,
                    CreatedDateTime = group.CreatedDateTime ?? DateTimeOffset.Now
                };

                groupList.Add(dbGroup);
                return Task.FromResult(true);
            });
    }

    private async Task StoreGroupToDb(GraphDbContext dbContext, List<DbGroup> groupList)
    {
        foreach (var item in groupList)
        {
            try
            {
                var dbItem = await dbContext.Groups.FindAsync(item.Id);

                if (dbItem == null)
                {
                    dbContext.Groups.Add(item);
                }
                else
                {
                    dbItem.DisplayName = item.DisplayName;
                    dbItem.Description = item.Description;
                    dbItem.MailNickname = item.MailNickname;
                    dbItem.CreatedDateTime = item.CreatedDateTime;
                    dbContext.Groups.Update(dbItem);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DbContext에 아이템 추가 중 에러 발생");
            }
        }
    }

    private PageIterator<GraphUser, UserCollectionResponse> GetUserPageIterator(List<DbUser> userList, UserCollectionResponse usersResponse)
    {
        return PageIterator<GraphUser, UserCollectionResponse>
            .CreatePageIterator(_graphClient, usersResponse, (user) =>
            {
                var dbUser = new DbUser
                {
                    Id = user.Id!,
                    DisplayName = user.DisplayName,
                    UserPrincipalName = user.UserPrincipalName,
                    MailNickname = user.MailNickname,
                    CreatedDateTime = user.CreatedDateTime ?? DateTimeOffset.Now
                };

                userList.Add(dbUser);
                return true;
            });
    }

    private async Task StoreUserToDb(GraphDbContext dbContext, List<DbUser> userList)
    {
        foreach (var item in userList)
        {
            try
            {
                var dbItem = await dbContext.Users.FindAsync(item.Id);

                if (dbItem == null)
                {
                    dbContext.Users.Add(item);
                }
                else
                {
                    dbItem.DisplayName = item.DisplayName;
                    dbItem.UserPrincipalName = item.UserPrincipalName;
                    dbItem.MailNickname = item.MailNickname;
                    dbItem.CreatedDateTime = item.CreatedDateTime;
                    dbContext.Users.Update(dbItem);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DbContext에 아이템 추가 중 에러 발생");
            }
        }
    }
}
