using Challenge04_TenantManagementApi.Data;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using DbUser = Challenge04_TenantManagementApi.Models.User;
using GraphUser = Microsoft.Graph.Models.User;
using DbGroup = Challenge04_TenantManagementApi.Models.Group;
using GraphGroup = Microsoft.Graph.Models.Group;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;

namespace Challenge04_TenantManagementApi.Services;

public sealed class TimedDataFetchingService : BackgroundService
{
    private readonly ILogger<TimedDataFetchingService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly GraphServiceClient _graphClient;

    public TimedDataFetchingService(ILogger<TimedDataFetchingService> logger, IServiceScopeFactory serviceScopeFactory, GraphServiceClient graphClient)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _graphClient = graphClient;
    }

    /// <summary>
    /// 앱이 실행되는 동안 유저와 그룹의 정보를 받아와 DB에 저장합니다.
    /// </summary>
    /// <param name="stoppingToken">작업을 중단시킬 수 있는 토큰</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Data Fetching Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Timed Data Fetching Service is working.");

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<GraphDbContext>();

                // Use dbContext to interact with the database here
                await FetchUserData(dbContext);
                await FetchGroupData(dbContext);

            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }

        _logger.LogInformation("Timed Data Fetching Service is stopping.");
    }

    private async Task FetchUserData(GraphDbContext dbContext)
    {
        var retryHandlerOption = new RetryHandlerOption
        {
            MaxRetry = 7,
            ShouldRetry = (delay, attempt, message) => true
        };

        var usersResponse = await _graphClient.Users.GetAsync(requestConfiguration =>
        {
            requestConfiguration.QueryParameters.Select = new[] { "id", "displayName", "userPrincipalName", "mailNickname" };
            requestConfiguration.Options.Add(retryHandlerOption);
        }) ?? throw new ArgumentNullException("usersResponse", "No users were found.");

        var pageIterator = PageIterator<GraphUser, UserCollectionResponse>
            .CreatePageIterator(_graphClient, usersResponse, async (user) =>
            {
                var dbUser = await dbContext.Users.FindAsync(user.Id);

                if (dbUser == null)
                {
                    var res = dbContext.Users.Add(new DbUser
                    {
                        Id = user.Id!,
                        DisplayName = user.DisplayName,
                        UserPrincipalName = user.UserPrincipalName,
                        MailNickname = user.MailNickname
                    });
                }
                else
                {
                    dbUser.DisplayName = user.DisplayName;
                    dbUser.UserPrincipalName = user.UserPrincipalName;
                    dbUser.MailNickname = user.MailNickname;
                    dbContext.Users.Update(dbUser);
                }

                return true;
            });

        await pageIterator.IterateAsync();
    }

    private async Task FetchGroupData(GraphDbContext dbContext)
    {
        var retryHandlerOption = new RetryHandlerOption
        {
            MaxRetry = 7,
            ShouldRetry = (delay, attempt, message) => true
        };

        var groupsResponse = await _graphClient.Groups.GetAsync(requestConfiguration =>
        {
            requestConfiguration.QueryParameters.Select = new[] { "id", "displayName", "description", "mailNickname" };
            requestConfiguration.Options.Add(retryHandlerOption);
        }) ?? throw new ArgumentNullException("groupsResponse", "No users were found.");

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
                    MailNickname = group.MailNickname
                });
            }
            else
            {
                dbGroup.DisplayName = group.DisplayName;
                dbGroup.Description = group.Description;
                dbGroup.MailNickname = group.MailNickname;
                dbContext.Groups.Update(dbGroup);
            }

            await dbContext.SaveChangesAsync();
            return true;
        });

        await pageIterator.IterateAsync();
    }
}
