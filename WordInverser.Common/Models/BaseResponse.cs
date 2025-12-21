namespace WordInverser.Common.Models;

public abstract class BaseResponse
{
    public Guid CorrelationId { get; set; }
    public DateTime ResponseTime { get; set; } = DateTime.UtcNow;
    public bool IsSuccess { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public List<string> Errors { get; set; } = new();
}
