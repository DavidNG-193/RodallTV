using DigitalSignage.Api.Configuration;
using Microsoft.Extensions.Options;

namespace DigitalSignage.Api.Services.References;

public sealed class DailyReferenceRefreshBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SagaOptions _options;
    private readonly ILogger<DailyReferenceRefreshBackgroundService> _logger;

    public DailyReferenceRefreshBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<SagaOptions> options,
        ILogger<DailyReferenceRefreshBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        TimeSpan interval = TimeSpan.FromMinutes(
            Math.Max(_options.RefreshMinutes, 1));

        using var timer = new PeriodicTimer(interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using IServiceScope scope = _scopeFactory.CreateScope();
                IDailyReferenceService service = scope.ServiceProvider
                    .GetRequiredService<IDailyReferenceService>();

                await service.RefreshAllAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Falló el ciclo automático de referencias.");
            }
        }
    }
}
