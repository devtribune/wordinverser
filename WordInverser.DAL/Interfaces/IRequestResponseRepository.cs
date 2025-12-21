using WordInverser.DAL.Entities;

namespace WordInverser.DAL.Interfaces;

public interface IRequestResponseRepository : IRepository<RequestResponse>
{
    Task<(IEnumerable<RequestResponse> data, int totalCount)> GetAllPagedAsync(int pageNumber, int pageSize);
    Task<(IEnumerable<RequestResponse> data, int totalCount)> SearchByWordAsync(string searchWord, int pageNumber, int pageSize);
}
