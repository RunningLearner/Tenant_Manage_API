using Microsoft.Graph;

namespace Challenge04_TenantManagementApi.Services;

public sealed class TimedDataFetchingService : BackgroundService
{
    private readonly ILogger<TimedDataFetchingService> _logger;
    private readonly GraphServiceClient _graphClient;

    public TimedDataFetchingService(ILogger<TimedDataFetchingService> logger, GraphServiceClient graphClient)
    {
        _logger = logger;
        _graphClient = graphClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Data Fetching Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Timed Data Fetching Service is working.");

            var users = await _graphClient.Users.GetAsync(cancellationToken: stoppingToken);

            // 로직 처리 부분입니다.

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }

        _logger.LogInformation("Timed Data Fetching Service is stopping.");
    }
}
