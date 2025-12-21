namespace WordInverser.Common.Models;

public class InverseWordsRequest : BaseRequest
{
    public string Sentence { get; set; } = string.Empty;
}
