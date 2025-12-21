using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WordInverser.API.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected readonly ILogger _logger;

    protected BaseController(ILogger logger)
    {
        _logger = logger;
    }

    protected IActionResult HandleException(Exception ex, Guid correlationId, string operation)
    {
        _logger.LogError(ex, $"Error in {operation}. CorrelationId: {correlationId}");

        return StatusCode(500, new
        {
            CorrelationId = correlationId,
            IsSuccess = false,
            ErrorMessage = "An internal error occurred while processing your request",
            Errors = new[] { ex.Message }
        });
    }

    protected IActionResult HandleBadRequest(string message, Guid correlationId)
    {
        _logger.LogWarning($"Bad request: {message}. CorrelationId: {correlationId}");

        return BadRequest(new
        {
            CorrelationId = correlationId,
            IsSuccess = false,
            ErrorMessage = message,
            Errors = new[] { message }
        });
    }
}
