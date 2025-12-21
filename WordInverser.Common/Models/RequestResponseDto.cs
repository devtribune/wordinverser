namespace WordInverser.Common.Models;

public class RequestResponseDto
{
    public int Id { get; set; }
    public Guid RequestId { get; set; }
    public string Request { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public bool IsSuccess { get; set; }
    public DateTime CreatedDate { get; set; }
    public long? ProcessingTimeMs { get; set; }
}
