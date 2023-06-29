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

    public DataFetchingService(GraphServiceClient graphClient)
    {
        _graphClient = graphClient;
    }

    /// <summary>
    /// Graph 클라이언트에서 유저들의 데이터를 가져와 db에 저장
    /// </summary>
    /// <param name="dbContext">현재 DB의 컨텍스트</param>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task FetchUserData(GraphDbContext dbContext)
    {
        var retryHandlerOption = new RetryHandlerOption
        {
            MaxRetry = 5,
        };

        var usersResponse = await _graphClient.Users.GetAsync(requestConfiguration =>
        {
            requestConfiguration.QueryParameters.Select = new[] { "id", "displayName", "userPrincipalName", "mailNickname", "createdDateTime" };
            requestConfiguration.Options.Add(retryHandlerOption);
        }) ?? throw new ServiceException("GraphClient가 응답하지 않습니다.");

        var pageIterator = PageIterator<GraphUser, UserCollectionResponse>
            .CreatePageIterator(_graphClient, usersResponse, async (user) =>
            {
                var dbUser = await dbContext.Users.FindAsync(user.Id);

                if (dbUser == null)
                {
                    dbContext.Users.Add(new DbUser
                    {
                        Id = user.Id!,
                        DisplayName = user.DisplayName,
                        UserPrincipalName = user.UserPrincipalName,
                        MailNickname = user.MailNickname,
                        CreatedDateTime = user.CreatedDateTime ?? DateTimeOffset.Now
                    });
                }
                else
                {
                    dbUser.DisplayName = user.DisplayName;
                    dbUser.UserPrincipalName = user.UserPrincipalName;
                    dbUser.MailNickname = user.MailNickname;
                    dbUser.CreatedDateTime = user.CreatedDateTime ?? DateTimeOffset.Now;
                    dbContext.Users.Update(dbUser);
                }

                await dbContext.SaveChangesAsync();
                return true;
            });

        await pageIterator.IterateAsync();
    }

    /// <summary>
    /// Graph 클라이언트에서 그룹들의 데이터를 가져와 db에 저장
    /// </summary>
    /// <param name="dbContext">현재 DB의 컨텍스트</param>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task FetchGroupData(GraphDbContext dbContext)
    {
        var retryHandlerOption = new RetryHandlerOption
        {
            MaxRetry = 7,
        };

        var groupsResponse = await _graphClient.Groups.GetAsync(requestConfiguration =>
        {
            requestConfiguration.QueryParameters.Select = new[] { "id", "displayName", "description", "mailNickname", "createdDateTime" };
            requestConfiguration.Options.Add(retryHandlerOption);
        }) ?? throw new ServiceException("GraphClient가 응답하지 않습니다.");

        var pageIterator = PageIterator<GraphGroup, GroupCollectionResponse>
        .CreatePageIterator(_graphClient, groupsResponse, async (group) =>
        {
            var dbGroup = await dbContext.Groups.FindAsync(group.Id);

            if (dbGroup == null)
            {
                dbContext.Groups.Add(new DbGroup
                {
                    Id = group.Id!,
                    DisplayName = group.DisplayName,
                    Description = group.Description,
                    MailNickname = group.MailNickname,
                    CreatedDateTime = group.CreatedDateTime ?? DateTimeOffset.Now
                });
            }
            else
            {
                dbGroup.DisplayName = group.DisplayName;
                dbGroup.Description = group.Description;
                dbGroup.MailNickname = group.MailNickname;
                dbGroup.CreatedDateTime = group.CreatedDateTime ?? DateTimeOffset.Now;
                dbContext.Groups.Update(dbGroup);
            }

            await dbContext.SaveChangesAsync();
            return true;
        });

        await pageIterator.IterateAsync();
    }
}
