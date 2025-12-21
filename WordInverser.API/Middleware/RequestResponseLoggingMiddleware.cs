using System.Diagnostics;
using System.Text;
using System.Text.Json;
using WordInverser.DAL.Entities;
using WordInverser.DAL.Interfaces;

namespace WordInverser.API.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IUnitOfWork unitOfWork)
    {
        // Only log for our API endpoints
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var requestBody = await ReadRequestBodyAsync(context.Request);
        var originalBodyStream = context.Response.Body;

        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        Exception? capturedException = null;
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            capturedException = ex;
            throw;
        }
        finally
        {
            stopwatch.Stop();

            var responseBody = await ReadResponseBodyAsync(context.Response);
            await responseBodyStream.CopyToAsync(originalBodyStream);

            // Save to database
            await SaveRequestResponseAsync(
                unitOfWork,
                requestBody,
                responseBody,
                stopwatch.ElapsedMilliseconds,
                capturedException,
                context.Response.StatusCode);
        }
    }

    private async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        request.EnableBuffering();

        using var reader = new StreamReader(
            request.Body,
            encoding: Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;

        return body;
    }

    private async Task<string> ReadResponseBodyAsync(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(response.Body).ReadToEndAsync();
        response.Body.Seek(0, SeekOrigin.Begin);

        return body;
    }

    private async Task SaveRequestResponseAsync(
        IUnitOfWork unitOfWork,
        string requestBody,
        string responseBody,
        long processingTimeMs,
        Exception? exception,
        int statusCode)
    {
        try
        {
            // Extract words from request for tagging
            var tags = ExtractWordsFromRequest(requestBody);

            var requestResponse = new RequestResponse
            {
                RequestId = Guid.NewGuid(),
                Request = requestBody,
                Response = responseBody,
                Tags = JsonSerializer.Serialize(tags),
                Exception = exception?.ToString(),
                IsSuccess = exception == null && statusCode >= 200 && statusCode < 300,
                CreatedDate = DateTime.UtcNow,
                ProcessingTimeMs = processingTimeMs
            };

            await unitOfWork.RequestResponseRepository.AddAsync(requestResponse);
            await unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"Request/Response logged successfully. RequestId: {requestResponse.RequestId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving request/response to database");
        }
    }

    private List<string> ExtractWordsFromRequest(string requestBody)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(requestBody))
                return new List<string>();

            using var doc = JsonDocument.Parse(requestBody);
            if (doc.RootElement.TryGetProperty("sentence", out var sentenceElement))
            {
                var sentence = sentenceElement.GetString();
                if (!string.IsNullOrWhiteSpace(sentence))
                {
                    return sentence
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Select(w => w.Trim())
                        .Where(w => !string.IsNullOrWhiteSpace(w))
                        .Distinct()
                        .ToList();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting words from request body");
        }

        return new List<string>();
    }
}

public static class RequestResponseLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestResponseLoggingMiddleware>();
    }
}
