using System.Text;

namespace WordInverser.Business.Services;

public class WordInverter
{
    /// <summary>
    /// Extracts the core word without special characters at boundaries and converts to lowercase for caching
    /// </summary>
    public static string GetNormalizedCoreWord(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
            return string.Empty;

        var chars = word.ToCharArray();
        int left = 0;
        int right = chars.Length - 1;

        // Find the first alphabetic character from the left
        while (left < chars.Length && !char.IsLetterOrDigit(chars[left]))
            left++;

        // Find the first alphabetic character from the right
        while (right >= 0 && !char.IsLetterOrDigit(chars[right]))
            right--;

        // If no alphabetic characters found
        if (left > right)
            return string.Empty;

        // Extract the middle part (including embedded special characters)
        var corePart = word.Substring(left, right - left + 1);
        
        // Convert to lowercase for consistent caching
        return corePart.ToLowerInvariant();
    }

    public static string InverseWord(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
            return word;

        var chars = word.ToCharArray();
        int left = 0;
        int right = chars.Length - 1;

        // Find the first alphabetic character from the left
        while (left < chars.Length && !char.IsLetterOrDigit(chars[left]))
            left++;

        // Find the first alphabetic character from the right
        while (right >= 0 && !char.IsLetterOrDigit(chars[right]))
            right--;

        // If no alphabetic characters found
        if (left >= right)
            return word;

        // Extract special characters at the beginning
        var prefixSpecialChars = word.Substring(0, left);

        // Extract special characters at the end
        var suffixSpecialChars = word.Substring(right + 1);

        // Extract the middle part to reverse (including special characters)
        var middlePart = word.Substring(left, right - left + 1);

        // Reverse the middle part
        var reversedMiddle = new string(middlePart.Reverse().ToArray());

        // Combine prefix + reversed middle + suffix
        return prefixSpecialChars + reversedMiddle + suffixSpecialChars;
    }

    public static string InverseSentence(string sentence)
    {
        if (string.IsNullOrWhiteSpace(sentence))
            return sentence;

        var words = sentence.Split(' ');
        var invertedWords = words.Select(InverseWord);
        return string.Join(' ', invertedWords);
    }
}
