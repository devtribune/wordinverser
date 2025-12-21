using WordInverser.Business.Interfaces;

namespace WordInverser.API.HostedServices;

public class CacheInitializationHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CacheInitializationHostedService> _logger;

    public CacheInitializationHostedService(
        IServiceProvider serviceProvider,
        ILogger<CacheInitializationHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cache initialization started");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var wordCacheService = scope.ServiceProvider.GetRequiredService<IWordCacheService>();

            await wordCacheService.LoadCacheAsync();

            _logger.LogInformation("Cache initialization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cache initialization");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cache initialization service stopped");
        return Task.CompletedTask;
    }
}
