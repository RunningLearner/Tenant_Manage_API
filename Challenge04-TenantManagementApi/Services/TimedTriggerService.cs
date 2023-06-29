using Challenge04_TenantManagementApi.Data;

namespace Challenge04_TenantManagementApi.Services;

public sealed class TimedTriggerService : BackgroundService
{
    private readonly ILogger<TimedTriggerService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public TimedTriggerService(ILogger<TimedTriggerService> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Data Fetching Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Data Fetching Service is working.");

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<GraphDbContext>();
                var dataFetchingService = scope.ServiceProvider.GetRequiredService<DataFetchingService>();

                await dataFetchingService.FetchUserData(dbContext);
                await dataFetchingService.FetchGroupData(dbContext);
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }

        _logger.LogInformation("Data Fetching Service is stopping.");
    }
}