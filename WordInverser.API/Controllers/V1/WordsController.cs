using Microsoft.AspNetCore.Mvc;
using WordInverser.Business.Interfaces;
using WordInverser.Common.Exceptions;
using WordInverser.Common.Models;

namespace WordInverser.API.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class WordsController : BaseController
{
    private readonly IWordInversionService _wordInversionService;

    public WordsController(
        IWordInversionService wordInversionService,
        ILogger<WordsController> logger) : base(logger)
    {
        _wordInversionService = wordInversionService;
    }

    /// <summary>
    /// Inverses all words in the provided sentence
    /// </summary>
    /// <param name="request">The sentence to inverse</param>
    /// <returns>The inversed sentence</returns>
    [HttpPost("inverse")]
    [ProducesResponseType(typeof(InverseWordsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> InverseWords([FromBody] InverseWordsRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Sentence))
            {
                return HandleBadRequest("Sentence cannot be empty", request?.CorrelationId ?? Guid.NewGuid());
            }

            _logger.LogInformation($"Processing word inversion request. CorrelationId: {request.CorrelationId}");

            var response = await _wordInversionService.InverseWordsAsync(request);

            if (!response.IsSuccess)
            {
                return StatusCode(500, response);
            }

            return Ok(response);
        }
        catch (CacheNotReadyException ex)
        {
            _logger.LogWarning(ex, "Cache not ready exception");
            return StatusCode(503, new
            {
                CorrelationId = request?.CorrelationId ?? Guid.NewGuid(),
                IsSuccess = false,
                ErrorMessage = ex.Message,
                Errors = new[] { "The application is still initializing. Please try again shortly." }
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex, request?.CorrelationId ?? Guid.NewGuid(), "InverseWords");
        }
    }
}
