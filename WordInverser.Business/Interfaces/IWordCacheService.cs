namespace WordInverser.Business.Interfaces;

public interface IWordCacheService
{
    Task<string?> GetInversedWordAsync(string word);
    Task LoadCacheAsync();
    Task CacheWordAsync(string word, string inversedWord);
    bool IsCacheReady { get; }
}
