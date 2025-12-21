using WordInverser.Common.Models;

namespace WordInverser.Business.Interfaces;

public interface IRequestResponseService
{
    Task<PagedResponse<RequestResponseDto>> GetAllRequestResponsesAsync(PagedRequest request);
    Task<PagedResponse<RequestResponseDto>> SearchByWordAsync(SearchRequestByWordRequest request);
}
