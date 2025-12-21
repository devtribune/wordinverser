using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WordInverser.DAL.Context;
using WordInverser.DAL.Entities;
using WordInverser.DAL.Interfaces;

namespace WordInverser.DAL.Repositories;

public class RequestResponseRepository : Repository<RequestResponse>, IRequestResponseRepository
{
    public RequestResponseRepository(WordInverserDbContext context) : base(context)
    {
    }

    public async Task<(IEnumerable<RequestResponse> data, int totalCount)> GetAllPagedAsync(int pageNumber, int pageSize)
    {
        var pageNumberParam = new SqlParameter("@PageNumber", pageNumber);
        var pageSizeParam = new SqlParameter("@PageSize", pageSize);

        var results = await _dbSet
            .FromSqlRaw("EXEC usp_GetAllRequestResponses @PageNumber, @PageSize", pageNumberParam, pageSizeParam)
            .ToListAsync();

        var totalCount = results.FirstOrDefault()?.Id ?? 0;
        if (results.Any())
        {
            // The stored procedure returns TotalCount in a computed column, but EF doesn't map it
            // We'll count separately for accuracy
            totalCount = await _dbSet.CountAsync();
        }

        return (results, totalCount);
    }

    public async Task<(IEnumerable<RequestResponse> data, int totalCount)> SearchByWordAsync(string searchWord, int pageNumber, int pageSize)
    {
        var searchWordParam = new SqlParameter("@SearchWord", searchWord);
        var pageNumberParam = new SqlParameter("@PageNumber", pageNumber);
        var pageSizeParam = new SqlParameter("@PageSize", pageSize);

        var results = await _dbSet
            .FromSqlRaw("EXEC usp_SearchRequestResponseByWord @SearchWord, @PageNumber, @PageSize",
                searchWordParam, pageNumberParam, pageSizeParam)
            .ToListAsync();

        var totalCount = results.Any()
            ? await _dbSet.CountAsync(r => r.Tags.Contains(searchWord))
            : 0;

        return (results, totalCount);
    }
}
