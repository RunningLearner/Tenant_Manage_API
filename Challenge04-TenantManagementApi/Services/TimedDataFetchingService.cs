using Challenge04_TenantManagementApi.Data;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using DbUser = Challenge04_TenantManagementApi.Models.User;
using GraphUser = Microsoft.Graph.Models.User;
using DbGroup = Challenge04_TenantManagementApi.Models.Group;
using GraphGroup = Microsoft.Graph.Models.Group;

namespace Challenge04_TenantManagementApi.Services;

public sealed class TimedDataFetchingService : BackgroundService
{
    private readonly ILogger<TimedDataFetchingService> _logger;
    private readonly GraphServiceClient _graphClient;

    private readonly GraphDbContext _dbContext;
    public TimedDataFetchingService(ILogger<TimedDataFetchingService> logger, GraphServiceClient graphClient, GraphDbContext dbContext)
    {
        _logger = logger;
        _graphClient = graphClient;
        _dbContext = dbContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Data Fetching Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Timed Data Fetching Service is working.");

            await FetchUserData();
            await FetchGroupData();

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }

        _logger.LogInformation("Timed Data Fetching Service is stopping.");
    }

    private async Task FetchUserData()
    {
        var usersResponse = await _graphClient.Users.GetAsync(requestConfiguration =>
         {
             requestConfiguration.QueryParameters.Select = new[] { "id", "displayName", "userPrincipalName", "mailNickname" };
         }) ?? throw new ArgumentNullException("usersResponse", "No users were found.");

        var pageIterator = PageIterator<GraphUser, UserCollectionResponse>
                    .CreatePageIterator(_graphClient, usersResponse, async (user) =>
                    {
                        var dbUser = await _dbContext.Users.FindAsync(user.Id);

                        if (dbUser == null)
                        {
                            _dbContext.Users.Add(new DbUser
                            {
                                Id = user.Id!,
                                DisplayName = user.DisplayName,
                                UserPrincipalName = user.UserPrincipalName,
                                MailNickname = user.MailNickname
                            });
                        }
                        else
                        {
                            _dbContext.Users.Update(new DbUser
                            {
                                Id = user.Id!,
                                DisplayName = user.DisplayName,
                                UserPrincipalName = user.UserPrincipalName,
                                MailNickname = user.MailNickname
                            });
                        }

                        return true;
                    });

        await pageIterator.IterateAsync();
    }

    private async Task FetchGroupData()
    {
        var groupsResponse = await _graphClient.Groups.GetAsync(requestConfiguration =>
         {
             requestConfiguration.QueryParameters.Select = new[] { "id", "displayName", "description", "mailNickname" };
         }) ?? throw new ArgumentNullException("groupsResponse", "No users were found.");

        var pageIterator = PageIterator<GraphGroup, GroupCollectionResponse>
                    .CreatePageIterator(_graphClient, groupsResponse, async (group) =>
                    {
                        var dbGroup = await _dbContext.Groups.FindAsync(group.Id);

                        if (dbGroup == null)
                        {
                            _dbContext.Groups.Add(new DbGroup
                            {
                                Id = group.Id!,
                                DisplayName = group.DisplayName,
                                Description = group.Description,
                                MailNickname = group.MailNickname
                            });
                        }
                        else
                        {
                            _dbContext.Groups.Update(new DbGroup
                            {
                                Id = group.Id!,
                                DisplayName = group.DisplayName,
                                Description = group.Description,
                                MailNickname = group.MailNickname
                            });
                        }

                        return true;
                    });

        await pageIterator.IterateAsync();
    }
}
