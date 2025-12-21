using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WordInverser.DAL.Context;
using WordInverser.DAL.Entities;
using WordInverser.DAL.Interfaces;

namespace WordInverser.DAL.Repositories;

public class WordCacheRepository : Repository<WordCache>, IWordCacheRepository
{
    public WordCacheRepository(WordInverserDbContext context) : base(context)
    {
    }

    public async Task<WordCache?> GetByWordAsync(string word)
    {
        return await _dbSet.FirstOrDefaultAsync(w => w.Word == word);
    }

    public async Task<IEnumerable<WordCache>> GetBatchAsync(int pageNumber, int pageSize)
    {
        var pageNumberParam = new SqlParameter("@PageNumber", pageNumber);
        var pageSizeParam = new SqlParameter("@PageSize", pageSize);

        return await _dbSet
            .FromSqlRaw("EXEC usp_GetWordCacheBatch @PageNumber, @PageSize", pageNumberParam, pageSizeParam)
            .ToListAsync();
    }

    public async Task UpsertAsync(string word, string inversedWord)
    {
        var wordParam = new SqlParameter("@Word", word);
        var inversedWordParam = new SqlParameter("@InversedWord", inversedWord);

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC usp_UpsertWordCache @Word, @InversedWord",
            wordParam,
            inversedWordParam);
    }
}
