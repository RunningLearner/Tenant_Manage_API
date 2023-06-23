using Microsoft.Graph;

namespace Challenge04_TenantManagementApi.Services;

public sealed class TimedDataFetchingService : IHostedService, IDisposable
{
    private readonly ILogger<TimedDataFetchingService> _logger;
    private readonly GraphServiceClient _graphClient;
    private Timer? _timer;
    private readonly TimeSpan _timeSpanToWork = TimeSpan.FromMinutes(5);
    public TimedDataFetchingService(ILogger<TimedDataFetchingService> logger, GraphServiceClient graphClient)
    {
        _logger = logger;
        _graphClient = graphClient;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Timed Data Fetching Service is starting.");

        _timer = new Timer(DoWork, null, TimeSpan.Zero, _timeSpanToWork);

        return Task.CompletedTask;
    }

    private async void DoWork(object state)
    {
        _logger.LogInformation("Timed Data Fetching Service is working.");

        var users = await _graphClient.Users.GetAsync();

        // 로직 처리 부분입니다.
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Timed Data Fetching Service is stopping.");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}