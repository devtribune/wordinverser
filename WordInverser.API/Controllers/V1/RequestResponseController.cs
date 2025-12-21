using Microsoft.AspNetCore.Mvc;
using WordInverser.Business.Interfaces;
using WordInverser.Common.Models;

namespace WordInverser.API.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class RequestResponseController : BaseController
{
    private readonly IRequestResponseService _requestResponseService;

    public RequestResponseController(
        IRequestResponseService requestResponseService,
        ILogger<RequestResponseController> logger) : base(logger)
    {
        _requestResponseService = requestResponseService;
    }

    /// <summary>
    /// Gets all request/response pairs with pagination
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paginated list of request/response pairs</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<RequestResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
            {
                return HandleBadRequest("Invalid pagination parameters. PageNumber must be >= 1 and PageSize must be between 1 and 100.", Guid.NewGuid());
            }

            var request = new PagedRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            _logger.LogInformation($"Retrieving all request responses. CorrelationId: {request.CorrelationId}");

            var response = await _requestResponseService.GetAllRequestResponsesAsync(request);

            if (!response.IsSuccess)
            {
                return StatusCode(500, response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            return HandleException(ex, Guid.NewGuid(), "GetAllRequestResponses");
        }
    }

    /// <summary>
    /// Searches request/response pairs by word with pagination
    /// </summary>
    /// <param name="searchWord">Word to search for</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paginated list of matching request/response pairs</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResponse<RequestResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchByWord([FromQuery] string searchWord, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchWord))
            {
                return HandleBadRequest("Search word cannot be empty", Guid.NewGuid());
            }

            if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
            {
                return HandleBadRequest("Invalid pagination parameters. PageNumber must be >= 1 and PageSize must be between 1 and 100.", Guid.NewGuid());
            }

            var request = new SearchRequestByWordRequest
            {
                SearchWord = searchWord,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            _logger.LogInformation($"Searching request responses by word: {searchWord}. CorrelationId: {request.CorrelationId}");

            var response = await _requestResponseService.SearchByWordAsync(request);

            if (!response.IsSuccess)
            {
                return StatusCode(500, response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            return HandleException(ex, Guid.NewGuid(), "SearchRequestResponsesByWord");
        }
    }
}
