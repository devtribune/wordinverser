using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WordInverser.API.Controllers.V1;
using WordInverser.Business.Interfaces;
using WordInverser.Common.Models;

namespace WordInverser.Tests.Controllers;

[TestClass]
public class RequestResponseControllerTests
{
    private Mock<IRequestResponseService> _mockService = null!;
    private Mock<ILogger<RequestResponseController>> _mockLogger = null!;
    private RequestResponseController _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockService = new Mock<IRequestResponseService>();
        _mockLogger = new Mock<ILogger<RequestResponseController>>();
        _controller = new RequestResponseController(_mockService.Object, _mockLogger.Object);
    }

    [TestMethod]
    public async Task GetAll_ValidRequest_ReturnsOkResult()
    {
        // Arrange
        var response = new PagedResponse<RequestResponseDto>
        {
            Data = new List<RequestResponseDto>(),
            TotalRecords = 0,
            IsSuccess = true
        };
        
        _mockService.Setup(x => x.GetAllRequestResponsesAsync(It.IsAny<PagedRequest>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetAll(1, 10);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = (OkObjectResult)result;
        var returnedResponse = okResult.Value as PagedResponse<RequestResponseDto>;
        Assert.IsNotNull(returnedResponse);
        Assert.IsTrue(returnedResponse.IsSuccess);
    }

    [TestMethod]
    public async Task GetAll_InvalidPageNumber_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetAll(0, 10);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task GetAll_InvalidPageSize_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetAll(1, 0);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task GetAll_PageSizeTooLarge_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetAll(1, 101);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task SearchByWord_ValidRequest_ReturnsOkResult()
    {
        // Arrange
        var response = new PagedResponse<RequestResponseDto>
        {
            Data = new List<RequestResponseDto>(),
            TotalRecords = 0,
            IsSuccess = true
        };
        
        _mockService.Setup(x => x.SearchByWordAsync(It.IsAny<SearchRequestByWordRequest>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.SearchByWord("test", 1, 10);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
    }

    [TestMethod]
    public async Task SearchByWord_EmptySearchWord_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.SearchByWord("", 1, 10);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task SearchByWord_InvalidPageNumber_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.SearchByWord("test", 0, 10);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task SearchByWord_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        _mockService.Setup(x => x.SearchByWordAsync(It.IsAny<SearchRequestByWordRequest>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.SearchByWord("test", 1, 10);

        // Assert
        Assert.IsInstanceOfType(result, typeof(ObjectResult));
        var objectResult = (ObjectResult)result;
        Assert.AreEqual(500, objectResult.StatusCode);
    }
}
