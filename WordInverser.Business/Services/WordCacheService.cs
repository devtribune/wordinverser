using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WordInverser.Business.Interfaces;
using WordInverser.DAL.Interfaces;

namespace WordInverser.Business.Services;

public class WordCacheService : IWordCacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WordCacheService> _logger;
    private bool _isCacheReady = false;
    private const int BatchSize = 1000;

    public bool IsCacheReady => _isCacheReady;

    public WordCacheService(
        IMemoryCache memoryCache,
        IServiceProvider serviceProvider,
        ILogger<WordCacheService> logger)
    {
        _memoryCache = memoryCache;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<string?> GetInversedWordAsync(string word)
    {
        if (_memoryCache.TryGetValue(word, out string? inversedWord))
        {
            return inversedWord;
        }

        return null;
    }

    public async Task LoadCacheAsync()
    {
        try
        {
            _logger.LogInformation("Starting to load word cache from database...");
            _isCacheReady = false;

            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            int pageNumber = 1;
            bool hasMoreData = true;
            int totalLoaded = 0;

            while (hasMoreData)
            {
                var batch = await unitOfWork.WordCacheRepository.GetBatchAsync(pageNumber, BatchSize);
                var batchList = batch.ToList();

                if (!batchList.Any())
                {
                    hasMoreData = false;
                    break;
                }

                foreach (var wordCache in batchList)
                {
                    _memoryCache.Set(wordCache.Word, wordCache.InversedWord, new MemoryCacheEntryOptions
                    {
                        Priority = CacheItemPriority.NeverRemove,
                        SlidingExpiration = TimeSpan.FromHours(24)
                    });
                    totalLoaded++;
                }

                _logger.LogInformation($"Loaded batch {pageNumber} with {batchList.Count} words. Total loaded: {totalLoaded}");
                pageNumber++;

                if (batchList.Count < BatchSize)
                {
                    hasMoreData = false;
                }

                // Add small delay to prevent database overload
                await Task.Delay(10);
            }

            _isCacheReady = true;
            _logger.LogInformation($"Word cache loaded successfully. Total words: {totalLoaded}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading word cache from database");
            _isCacheReady = false;
            throw;
        }
    }

    public async Task CacheWordAsync(string word, string inversedWord)
    {
        _memoryCache.Set(word, inversedWord, new MemoryCacheEntryOptions
        {
            Priority = CacheItemPriority.NeverRemove,
            SlidingExpiration = TimeSpan.FromHours(24)
        });

        // Save to database asynchronously using a new scope
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await unitOfWork.WordCacheRepository.UpsertAsync(word, inversedWord);
        await unitOfWork.SaveChangesAsync();
    }
}
