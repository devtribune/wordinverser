namespace WordInverser.Common.Models;

public abstract class BaseRequest
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public DateTime RequestTime { get; set; } = DateTime.UtcNow;
}
