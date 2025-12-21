using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WordInverser.API.Controllers.V1;
using WordInverser.Business.Interfaces;
using WordInverser.Common.Exceptions;
using WordInverser.Common.Models;

namespace WordInverser.Tests.Controllers;

[TestClass]
public class WordsControllerTests
{
    private Mock<IWordInversionService> _mockService = null!;
    private Mock<ILogger<WordsController>> _mockLogger = null!;
    private WordsController _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockService = new Mock<IWordInversionService>();
        _mockLogger = new Mock<ILogger<WordsController>>();
        _controller = new WordsController(_mockService.Object, _mockLogger.Object);
    }

    [TestMethod]
    public async Task InverseWords_ValidRequest_ReturnsOkResult()
    {
        // Arrange
        var request = new InverseWordsRequest { Sentence = "hello" };
        var response = new InverseWordsResponse 
        { 
            InversedSentence = "olleh",
            IsSuccess = true,
            ProcessingTimeMs = 10
        };
        
        _mockService.Setup(x => x.InverseWordsAsync(It.IsAny<InverseWordsRequest>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.InverseWords(request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = (OkObjectResult)result;
        var returnedResponse = okResult.Value as InverseWordsResponse;
        Assert.IsNotNull(returnedResponse);
        Assert.AreEqual("olleh", returnedResponse.InversedSentence);
    }

    [TestMethod]
    public async Task InverseWords_EmptySentence_ReturnsBadRequest()
    {
        // Arrange
        var request = new InverseWordsRequest { Sentence = "" };

        // Act
        var result = await _controller.InverseWords(request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task InverseWords_NullSentence_ReturnsBadRequest()
    {
        // Arrange
        var request = new InverseWordsRequest { Sentence = null! };

        // Act
        var result = await _controller.InverseWords(request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task InverseWords_WhitespaceSentence_ReturnsBadRequest()
    {
        // Arrange
        var request = new InverseWordsRequest { Sentence = "   " };

        // Act
        var result = await _controller.InverseWords(request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task InverseWords_CacheNotReady_ReturnsServiceUnavailable()
    {
        // Arrange
        var request = new InverseWordsRequest { Sentence = "hello" };
        _mockService.Setup(x => x.InverseWordsAsync(It.IsAny<InverseWordsRequest>()))
            .ThrowsAsync(new CacheNotReadyException());

        // Act
        var result = await _controller.InverseWords(request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(ObjectResult));
        var objectResult = (ObjectResult)result;
        Assert.AreEqual(503, objectResult.StatusCode);
    }

    [TestMethod]
    public async Task InverseWords_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var request = new InverseWordsRequest { Sentence = "hello" };
        _mockService.Setup(x => x.InverseWordsAsync(It.IsAny<InverseWordsRequest>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.InverseWords(request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(ObjectResult));
        var objectResult = (ObjectResult)result;
        Assert.AreEqual(500, objectResult.StatusCode);
    }
}
