namespace WordInverser.Common.Models;

public class InverseWordsResponse : BaseResponse
{
    public string InversedSentence { get; set; } = string.Empty;
    public long ProcessingTimeMs { get; set; }
}
