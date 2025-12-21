using WordInverser.Common.Models;

namespace WordInverser.Business.Interfaces;

public interface IWordInversionService
{
    Task<InverseWordsResponse> InverseWordsAsync(InverseWordsRequest request);
}
