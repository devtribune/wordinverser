using Microsoft.Extensions.Logging;
using WordInverser.Business.Interfaces;
using WordInverser.Common.Models;
using WordInverser.DAL.Interfaces;

namespace WordInverser.Business.Services;

public class RequestResponseService : IRequestResponseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RequestResponseService> _logger;

    public RequestResponseService(
        IUnitOfWork unitOfWork,
        ILogger<RequestResponseService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PagedResponse<RequestResponseDto>> GetAllRequestResponsesAsync(PagedRequest request)
    {
        try
        {
            var (data, totalCount) = await _unitOfWork.RequestResponseRepository
                .GetAllPagedAsync(request.PageNumber, request.PageSize);

            var dtoList = data.Select(r => new RequestResponseDto
            {
                Id = r.Id,
                RequestId = r.RequestId,
                Request = r.Request,
                Response = r.Response,
                Tags = r.Tags,
                Exception = r.Exception,
                IsSuccess = r.IsSuccess,
                CreatedDate = r.CreatedDate,
                ProcessingTimeMs = r.ProcessingTimeMs
            }).ToList();

            return new PagedResponse<RequestResponseDto>
            {
                CorrelationId = request.CorrelationId,
                Data = dtoList,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalRecords = totalCount,
                IsSuccess = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all request responses");
            return new PagedResponse<RequestResponseDto>
            {
                CorrelationId = request.CorrelationId,
                IsSuccess = false,
                ErrorMessage = "An error occurred while retrieving request responses",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<PagedResponse<RequestResponseDto>> SearchByWordAsync(SearchRequestByWordRequest request)
    {
        try
        {
            var (data, totalCount) = await _unitOfWork.RequestResponseRepository
                .SearchByWordAsync(request.SearchWord, request.PageNumber, request.PageSize);

            var dtoList = data.Select(r => new RequestResponseDto
            {
                Id = r.Id,
                RequestId = r.RequestId,
                Request = r.Request,
                Response = r.Response,
                Tags = r.Tags,
                Exception = r.Exception,
                IsSuccess = r.IsSuccess,
                CreatedDate = r.CreatedDate,
                ProcessingTimeMs = r.ProcessingTimeMs
            }).ToList();

            return new PagedResponse<RequestResponseDto>
            {
                CorrelationId = request.CorrelationId,
                Data = dtoList,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalRecords = totalCount,
                IsSuccess = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error searching request responses by word: {request.SearchWord}");
            return new PagedResponse<RequestResponseDto>
            {
                CorrelationId = request.CorrelationId,
                IsSuccess = false,
                ErrorMessage = "An error occurred while searching request responses",
                Errors = new List<string> { ex.Message }
            };
        }
    }
}
