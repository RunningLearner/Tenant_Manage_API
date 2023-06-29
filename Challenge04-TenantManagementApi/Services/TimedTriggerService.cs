using Challenge04_TenantManagementApi.Data;
using Timer = System.Timers.Timer;

namespace Challenge04_TenantManagementApi.Services;

public sealed class TimedTriggerService : BackgroundService
{
    private readonly ILogger<TimedTriggerService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private Timer? _timer;
    private bool _isProcessing;

    public TimedTriggerService(ILogger<TimedTriggerService> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("데이터를 반복적으로 가져오기 시작합니다.");

        _timer = new Timer(TimeSpan.FromMinutes(5).TotalMilliseconds); // 5 minutes
        _timer.Elapsed += async (sender, e) => await ProcessData();
        _timer.Start();

        return Task.CompletedTask;
    }

    private async Task ProcessData()
    {
        if (_isProcessing)
        {
            _logger.LogInformation("데이터를 가져오는 작업이 완료되지 않았습니다.");
            return;
        }

        _isProcessing = true;

        try
        {
            _logger.LogInformation("데이터를 가져오는 중입니다.");

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<GraphDbContext>();
                var dataFetchingService = scope.ServiceProvider.GetRequiredService<DataFetchingService>();

                await dataFetchingService.FetchUserData(dbContext);
                await dataFetchingService.FetchGroupData(dbContext);
            }

            _logger.LogInformation("데이터를 가져왔습니다.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "데이터를 Graph api에서 가져오는데 문제가 발생했습니다.");
        }
        finally
        {
            _isProcessing = false;
        }
    }
}