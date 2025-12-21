using Microsoft.Extensions.Logging;
using System.Diagnostics;
using WordInverser.Business.Interfaces;
using WordInverser.Common.Exceptions;
using WordInverser.Common.Models;

namespace WordInverser.Business.Services;

public class WordInversionService : IWordInversionService
{
    private readonly IWordCacheService _wordCacheService;
    private readonly ILogger<WordInversionService> _logger;

    public WordInversionService(
        IWordCacheService wordCacheService,
        ILogger<WordInversionService> logger)
    {
        _wordCacheService = wordCacheService;
        _logger = logger;
    }

    private static string ReconstructWordWithSpecialChars(string originalWord, string inversedCore)
    {
        if (string.IsNullOrWhiteSpace(originalWord))
            return originalWord;

        var chars = originalWord.ToCharArray();
        int left = 0;
        int right = chars.Length - 1;

        // Find the first alphabetic character from the left
        while (left < chars.Length && !char.IsLetterOrDigit(chars[left]))
            left++;

        // Find the first alphabetic character from the right
        while (right >= 0 && !char.IsLetterOrDigit(chars[right]))
            right--;

        if (left > right)
            return originalWord;

        // Extract special characters at the beginning and end
        var prefixSpecialChars = originalWord.Substring(0, left);
        var suffixSpecialChars = originalWord.Substring(right + 1);

        // Combine prefix + inversed core + suffix
        return prefixSpecialChars + inversedCore + suffixSpecialChars;
    }

    public async Task<InverseWordsResponse> InverseWordsAsync(InverseWordsRequest request)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            if (!_wordCacheService.IsCacheReady)
            {
                throw new CacheNotReadyException();
            }

            if (string.IsNullOrWhiteSpace(request.Sentence))
            {
                return new InverseWordsResponse
                {
                    CorrelationId = request.CorrelationId,
                    InversedSentence = string.Empty,
                    ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
                    IsSuccess = true
                };
            }

            var words = request.Sentence.Split(' ', StringSplitOptions.None);
            var inversedWords = new List<string>();

            foreach (var word in words)
            {
                if (string.IsNullOrWhiteSpace(word))
                {
                    inversedWords.Add(word);
                    continue;
                }

                // Get normalized core word (lowercase, no boundary special chars) for cache lookup
                var normalizedWord = WordInverter.GetNormalizedCoreWord(word);
                
                if (string.IsNullOrEmpty(normalizedWord))
                {
                    // Word contains only special characters, no need to inverse
                    inversedWords.Add(word);
                    continue;
                }

                // Check cache using normalized word
                var cachedInverse = await _wordCacheService.GetInversedWordAsync(normalizedWord);
                if (cachedInverse != null)
                {
                    // Reconstruct with original special characters
                    var originalInversed = ReconstructWordWithSpecialChars(word, cachedInverse);
                    inversedWords.Add(originalInversed);
                }
                else
                {
                    // Inverse the word
                    var inversedWord = WordInverter.InverseWord(word);
                    inversedWords.Add(inversedWord);

                    // Cache normalized core word with its inversed core (fire and forget)
                    var inversedCore = WordInverter.GetNormalizedCoreWord(inversedWord);
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _wordCacheService.CacheWordAsync(normalizedWord, inversedCore);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, $"Failed to cache word: {normalizedWord}");
                        }
                    });
                }
            }

            stopwatch.Stop();

            return new InverseWordsResponse
            {
                CorrelationId = request.CorrelationId,
                InversedSentence = string.Join(' ', inversedWords),
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
                IsSuccess = true
            };
        }
        catch (CacheNotReadyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inverting words");
            stopwatch.Stop();

            return new InverseWordsResponse
            {
                CorrelationId = request.CorrelationId,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
                IsSuccess = false,
                ErrorMessage = "An error occurred while processing your request",
                Errors = new List<string> { ex.Message }
            };
        }
    }
}
