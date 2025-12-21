namespace WordInverser.Common.Models;

public class SearchRequestByWordRequest : PagedRequest
{
    public string SearchWord { get; set; } = string.Empty;
}
