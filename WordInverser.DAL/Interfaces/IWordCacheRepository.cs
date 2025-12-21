using WordInverser.DAL.Entities;

namespace WordInverser.DAL.Interfaces;

public interface IWordCacheRepository : IRepository<WordCache>
{
    Task<WordCache?> GetByWordAsync(string word);
    Task<IEnumerable<WordCache>> GetBatchAsync(int pageNumber, int pageSize);
    Task UpsertAsync(string word, string inversedWord);
}
