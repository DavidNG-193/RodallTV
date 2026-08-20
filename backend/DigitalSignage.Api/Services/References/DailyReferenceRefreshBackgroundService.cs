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

        _logger.LogInformation(
            "Actualización automática de referencias iniciada con intervalo de {RefreshMinutes} minutos.",
            interval.TotalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using IServiceScope scope = _scopeFactory.CreateScope();
                IDailyReferenceService service = scope.ServiceProvider
                    .GetRequiredService<IDailyReferenceService>();

                var result = await service.RefreshAllAsync(stoppingToken);

                if (result.FailedCount > 0 || result.NotFoundCount > 0)
                {
                    _logger.LogWarning(
                        "Ciclo automático de referencias incompleto. Total: {TotalCount}, actualizadas: {RefreshedCount}, no encontradas: {NotFoundCount}, fallidas: {FailedCount}.",
                        result.TotalCount,
                        result.RefreshedCount,
                        result.NotFoundCount,
                        result.FailedCount);
                }
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

            try
            {
                await Task.Delay(interval, stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
