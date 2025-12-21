using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WordInverser.Business.Interfaces;
using WordInverser.Business.Services;
using WordInverser.Common.Exceptions;
using WordInverser.Common.Models;

namespace WordInverser.Tests.Business;

[TestClass]
public class WordInversionServiceTests
{
    private Mock<IWordCacheService> _mockCacheService = null!;
    private Mock<ILogger<WordInversionService>> _mockLogger = null!;
    private WordInversionService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockCacheService = new Mock<IWordCacheService>();
        _mockLogger = new Mock<ILogger<WordInversionService>>();
        _service = new WordInversionService(_mockCacheService.Object, _mockLogger.Object);
    }

    [TestMethod]
    public async Task InverseWordsAsync_CacheNotReady_ThrowsCacheNotReadyException()
    {
        // Arrange
        _mockCacheService.Setup(x => x.IsCacheReady).Returns(false);
        var request = new InverseWordsRequest { Sentence = "hello" };

        // Act & Assert
        await Assert.ThrowsAsync<CacheNotReadyException>(
            async () => await _service.InverseWordsAsync(request));
    }

    [TestMethod]
    public async Task InverseWordsAsync_EmptySentence_ReturnsEmptyResult()
    {
        // Arrange
        _mockCacheService.Setup(x => x.IsCacheReady).Returns(true);
        var request = new InverseWordsRequest { Sentence = "" };

        // Act
        var result = await _service.InverseWordsAsync(request);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("", result.InversedSentence);
    }

    [TestMethod]
    public async Task InverseWordsAsync_WordInCache_UsesCachedValue()
    {
        // Arrange
        _mockCacheService.Setup(x => x.IsCacheReady).Returns(true);
        _mockCacheService.Setup(x => x.GetInversedWordAsync("hello"))
            .ReturnsAsync("olleh");
        
        var request = new InverseWordsRequest { Sentence = "!hello?" };

        // Act
        var result = await _service.InverseWordsAsync(request);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("!olleh?", result.InversedSentence);
        _mockCacheService.Verify(x => x.GetInversedWordAsync("hello"), Times.Once);
    }

    [TestMethod]
    public async Task InverseWordsAsync_WordNotInCache_InversesAndCaches()
    {
        // Arrange
        _mockCacheService.Setup(x => x.IsCacheReady).Returns(true);
        _mockCacheService.Setup(x => x.GetInversedWordAsync(It.IsAny<string>()))
            .ReturnsAsync((string?)null);
        
        var request = new InverseWordsRequest { Sentence = "hello" };

        // Act
        var result = await _service.InverseWordsAsync(request);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("olleh", result.InversedSentence);
        
        // Give async task time to complete
        await Task.Delay(100);
        _mockCacheService.Verify(x => x.CacheWordAsync("hello", "olleh"), Times.Once);
    }

    [TestMethod]
    public async Task InverseWordsAsync_MultiplWords_InversesAll()
    {
        // Arrange
        _mockCacheService.Setup(x => x.IsCacheReady).Returns(true);
        _mockCacheService.Setup(x => x.GetInversedWordAsync(It.IsAny<string>()))
            .ReturnsAsync((string?)null);
        
        var request = new InverseWordsRequest { Sentence = "hello world" };

        // Act
        var result = await _service.InverseWordsAsync(request);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("olleh dlrow", result.InversedSentence);
    }

    [TestMethod]
    public async Task InverseWordsAsync_MixedCaseWithSpecialChars_HandlesCorrectly()
    {
        // Arrange
        _mockCacheService.Setup(x => x.IsCacheReady).Returns(true);
        _mockCacheService.Setup(x => x.GetInversedWordAsync(It.IsAny<string>()))
            .ReturnsAsync((string?)null);
        
        var request = new InverseWordsRequest { Sentence = "!Hello WORLD?" };

        // Act
        var result = await _service.InverseWordsAsync(request);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("!olleH DLROW?", result.InversedSentence);
    }

    [TestMethod]
    public async Task InverseWordsAsync_SetsProcessingTime()
    {
        // Arrange
        _mockCacheService.Setup(x => x.IsCacheReady).Returns(true);
        _mockCacheService.Setup(x => x.GetInversedWordAsync(It.IsAny<string>()))
            .ReturnsAsync((string?)null);
        
        var request = new InverseWordsRequest { Sentence = "hello" };

        // Act
        var result = await _service.InverseWordsAsync(request);

        // Assert
        Assert.IsTrue(result.ProcessingTimeMs >= 0);
    }
}
