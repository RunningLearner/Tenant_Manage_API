using Challenge04_TenantManagementApi.Data;
using Timer = System.Timers.Timer;

namespace Challenge04_TenantManagementApi.Services;

public sealed class TimedBackgroundService : IHostedService, IDisposable
{
    private const string ClassName = nameof(TimedBackgroundService);
    private readonly ILogger<TimedBackgroundService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly Timer _timer;
    private bool _isProcessing;

    public TimedBackgroundService(ILogger<TimedBackgroundService> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _timer = new Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
        _timer.Elapsed += async (sender, e) => await ProcessData();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[{ClassName}] 서비스 시작", ClassName);
        _timer.Start();
        Task.Run(ProcessData, cancellationToken); // 처음에 바로 동작하도록 
        return Task.CompletedTask;
    }

    private async Task ProcessData()
    {
        if (_isProcessing)
        {
            _logger.LogInformation("작업 중복에 의한 건너뛰기");
            return;
        }

        _isProcessing = true;

        _logger.LogInformation("데이터를 가져오는 중");

        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<GraphDbContext>();
            var dataFetchingService = scope.ServiceProvider.GetRequiredService<DataFetchingService>();

            await dataFetchingService.FetchData(dbContext);
        }

        _logger.LogInformation("데이터 가져오기 완료");

        _isProcessing = false;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[{ClassName}] 서비스 종료", ClassName);
        Dispose();
        return Task.CompletedTask;
    }

    /// <summary>
    /// 메모리 릭을 방지하는 메서드
    /// </summary>
    public void Dispose()
    {
        _timer.Close();
    }
}