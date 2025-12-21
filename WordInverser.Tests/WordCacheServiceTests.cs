using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WordInverser.Business.Services;
using WordInverser.DAL.Entities;
using WordInverser.DAL.Interfaces;

namespace WordInverser.Tests.Business;

[TestClass]
public class WordCacheServiceTests
{
    private Mock<IServiceProvider> _mockServiceProvider = null!;
    private Mock<IServiceScope> _mockScope = null!;
    private Mock<IServiceScopeFactory> _mockScopeFactory = null!;
    private Mock<IUnitOfWork> _mockUnitOfWork = null!;
    private Mock<IWordCacheRepository> _mockRepository = null!;
    private Mock<ILogger<WordCacheService>> _mockLogger = null!;
    private IMemoryCache _memoryCache = null!;
    private WordCacheService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockScope = new Mock<IServiceScope>();
        _mockScopeFactory = new Mock<IServiceScopeFactory>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockRepository = new Mock<IWordCacheRepository>();
        _mockLogger = new Mock<ILogger<WordCacheService>>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());

        // Setup the scope chain
        var scopedServiceProvider = new Mock<IServiceProvider>();
        scopedServiceProvider.Setup(x => x.GetService(typeof(IUnitOfWork)))
            .Returns(_mockUnitOfWork.Object);
        
        _mockScope.Setup(x => x.ServiceProvider).Returns(scopedServiceProvider.Object);
        
        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(_mockScope.Object);
        
        _mockServiceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(_mockScopeFactory.Object);
        
        _mockUnitOfWork.Setup(x => x.WordCacheRepository).Returns(_mockRepository.Object);

        _service = new WordCacheService(_memoryCache, _mockServiceProvider.Object, _mockLogger.Object);
    }

    [TestMethod]
    public async Task LoadCacheAsync_EmptyDatabase_SetsCacheReady()
    {
        // Arrange
        _mockRepository.Setup(x => x.GetBatchAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<WordCache>());

        // Act
        await _service.LoadCacheAsync();

        // Assert
        Assert.IsTrue(_service.IsCacheReady);
    }

    [TestMethod]
    public async Task LoadCacheAsync_WithData_LoadsIntoCache()
    {
        // Arrange
        var batch1 = new List<WordCache>
        {
            new WordCache { Word = "hello", InversedWord = "olleh" },
            new WordCache { Word = "world", InversedWord = "dlrow" }
        };
        
        _mockRepository.SetupSequence(x => x.GetBatchAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(batch1)
            .ReturnsAsync(new List<WordCache>());

        // Act
        await _service.LoadCacheAsync();

        // Assert
        Assert.IsTrue(_service.IsCacheReady);
        var result1 = await _service.GetInversedWordAsync("hello");
        var result2 = await _service.GetInversedWordAsync("world");
        Assert.AreEqual("olleh", result1);
        Assert.AreEqual("dlrow", result2);
    }

    [TestMethod]
    public async Task LoadCacheAsync_DatabaseError_ThrowsException()
    {
        // Arrange
        _mockRepository.Setup(x => x.GetBatchAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(
            async () => await _service.LoadCacheAsync());
        Assert.IsFalse(_service.IsCacheReady);
    }

    [TestMethod]
    public async Task GetInversedWordAsync_WordInCache_ReturnsValue()
    {
        // Arrange
        _memoryCache.Set("test", "tset");

        // Act
        var result = await _service.GetInversedWordAsync("test");

        // Assert
        Assert.AreEqual("tset", result);
    }

    [TestMethod]
    public async Task GetInversedWordAsync_WordNotInCache_ReturnsNull()
    {
        // Act
        var result = await _service.GetInversedWordAsync("notfound");

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task CacheWordAsync_AddsToMemoryCacheAndDatabase()
    {
        // Act
        await _service.CacheWordAsync("new", "wen");

        // Assert
        var cached = await _service.GetInversedWordAsync("new");
        Assert.AreEqual("wen", cached);
        _mockRepository.Verify(x => x.UpsertAsync("new", "wen"), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [TestMethod]
    public void IsCacheReady_InitialState_ReturnsFalse()
    {
        // Assert
        Assert.IsFalse(_service.IsCacheReady);
    }
}
