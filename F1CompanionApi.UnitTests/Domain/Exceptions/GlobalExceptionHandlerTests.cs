using F1CompanionApi.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Npgsql;

namespace F1CompanionApi.UnitTests.Domain.Exceptions;

public class GlobalExceptionHandlerTests
{
    private readonly Mock<ILogger<GlobalExceptionHandler>> _mockLogger;
    private readonly Mock<IProblemDetailsService> _mockProblemDetailsService;
    private readonly GlobalExceptionHandler _handler;
    private readonly DefaultHttpContext _httpContext;

    public GlobalExceptionHandlerTests()
    {
        _mockLogger = new Mock<ILogger<GlobalExceptionHandler>>();
        _mockProblemDetailsService = new Mock<IProblemDetailsService>();
        _handler = new GlobalExceptionHandler(_mockLogger.Object, _mockProblemDetailsService.Object);
        _httpContext = new DefaultHttpContext();
    }

    [Theory]
    [InlineData("42P01", "relation \"teams\" does not exist", 503, "Service Configuration Error", "The service is not properly configured. Please contact support.", true, LogLevel.Error)]
    [InlineData("23505", "duplicate key value violates unique constraint", 409, "Duplicate Resource", "This resource already exists.", false, LogLevel.Warning)]
    [InlineData("23503", "insert or update violates foreign key constraint", 400, "Invalid Reference", "The referenced resource does not exist.", false, LogLevel.Warning)]
    [InlineData("23502", "null value in column violates not-null constraint", 400, "Missing Required Field", "A required field is missing.", false, LogLevel.Warning)]
    [InlineData("99999", "unknown database error", 500, "Database Error", "A database error occurred. Please try again later.", true, LogLevel.Error)]
    public async Task TryHandleAsync_PostgresException_ReturnsExpectedStatusAndProblemDetails(
        string sqlState,
        string errorMessage,
        int expectedStatusCode,
        string expectedTitle,
        string expectedDetail,
        bool shouldIncludeException,
        LogLevel expectedLogLevel)
    {
        // Arrange
        var pgEx = new PostgresException(errorMessage, "ERROR", "ERROR", sqlState);
        var dbEx = new DbUpdateException("Update failed", pgEx);

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, dbEx, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedStatusCode, _httpContext.Response.StatusCode);

        // Verify ProblemDetails was written with correct values
        _mockProblemDetailsService.Verify(x => x.WriteAsync(
            It.Is<ProblemDetailsContext>(ctx =>
                ctx.HttpContext == _httpContext &&
                ctx.ProblemDetails.Status == expectedStatusCode &&
                ctx.ProblemDetails.Title == expectedTitle &&
                ctx.ProblemDetails.Detail == expectedDetail &&
                ctx.ProblemDetails.Type == $"https://httpstatuses.com/{expectedStatusCode}" &&
                ctx.Exception == (shouldIncludeException ? dbEx : null)
            )
        ), Times.Once);

        // Verify logging at appropriate level
        _mockLogger.Verify(
            x => x.Log(
                expectedLogLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                dbEx,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("42P01", "relation \"TeamDrivers\" does not exist", 503, "Service Configuration Error", "The service is not properly configured. Please contact support.", true, LogLevel.Error)]
    [InlineData("23505", "duplicate key value violates unique constraint", 409, "Duplicate Resource", "This resource already exists.", false, LogLevel.Warning)]
    public async Task TryHandleAsync_DirectPostgresException_ReturnsExpectedStatusAndProblemDetails(
        string sqlState,
        string errorMessage,
        int expectedStatusCode,
        string expectedTitle,
        string expectedDetail,
        bool shouldIncludeException,
        LogLevel expectedLogLevel)
    {
        // Arrange - Direct PostgresException (not wrapped in DbUpdateException)
        var pgEx = new PostgresException(errorMessage, "ERROR", "ERROR", sqlState);

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, pgEx, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedStatusCode, _httpContext.Response.StatusCode);

        // Verify ProblemDetails was written with correct values
        _mockProblemDetailsService.Verify(x => x.WriteAsync(
            It.Is<ProblemDetailsContext>(ctx =>
                ctx.HttpContext == _httpContext &&
                ctx.ProblemDetails.Status == expectedStatusCode &&
                ctx.ProblemDetails.Title == expectedTitle &&
                ctx.ProblemDetails.Detail == expectedDetail &&
                ctx.ProblemDetails.Type == $"https://httpstatuses.com/{expectedStatusCode}" &&
                ctx.Exception == (shouldIncludeException ? pgEx : null)
            )
        ), Times.Once);

        // Verify logging at appropriate level
        _mockLogger.Verify(
            x => x.Log(
                expectedLogLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                pgEx,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_InvalidOperationWithUserProfile_Returns400WithUserProfileRequired()
    {
        // Arrange
        var ex = new InvalidOperationException("User profile not found for authenticated user");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, ex, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(StatusCodes.Status400BadRequest, _httpContext.Response.StatusCode);

        _mockProblemDetailsService.Verify(x => x.WriteAsync(
            It.Is<ProblemDetailsContext>(ctx =>
                ctx.ProblemDetails.Title == "User Profile Required" &&
                ctx.ProblemDetails.Detail == "Please complete your registration before accessing this resource."
            )
        ), Times.Once);

        // Verify warning logging for 4xx
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                ex,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_InvalidOperationWithUserId_Returns401WithAuthenticationRequired()
    {
        // Arrange
        var ex = new InvalidOperationException("User ID not found in token");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, ex, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, _httpContext.Response.StatusCode);

        _mockProblemDetailsService.Verify(x => x.WriteAsync(
            It.Is<ProblemDetailsContext>(ctx =>
                ctx.ProblemDetails.Title == "Authentication Required" &&
                ctx.ProblemDetails.Detail == "Valid authentication token is required."
            )
        ), Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_DbUpdateConcurrencyException_Returns409WithConcurrencyConflict()
    {
        // Arrange
        var ex = new DbUpdateConcurrencyException("Concurrency conflict");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, ex, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(StatusCodes.Status409Conflict, _httpContext.Response.StatusCode);

        _mockProblemDetailsService.Verify(x => x.WriteAsync(
            It.Is<ProblemDetailsContext>(ctx =>
                ctx.ProblemDetails.Title == "Concurrency Conflict" &&
                ctx.ProblemDetails.Detail == "The data was modified by another user. Please refresh and try again."
            )
        ), Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_GenericInvalidOperationException_Returns400WithInvalidOperation()
    {
        // Arrange
        var ex = new InvalidOperationException("Some business rule violation");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, ex, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(StatusCodes.Status400BadRequest, _httpContext.Response.StatusCode);

        _mockProblemDetailsService.Verify(x => x.WriteAsync(
            It.Is<ProblemDetailsContext>(ctx =>
                ctx.ProblemDetails.Title == "Invalid Operation" &&
                ctx.ProblemDetails.Detail == "Some business rule violation"
            )
        ), Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_KeyNotFoundException_Returns404WithResourceNotFound()
    {
        // Arrange
        var ex = new KeyNotFoundException("Entity with ID 123 not found");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, ex, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(StatusCodes.Status404NotFound, _httpContext.Response.StatusCode);

        _mockProblemDetailsService.Verify(x => x.WriteAsync(
            It.Is<ProblemDetailsContext>(ctx =>
                ctx.ProblemDetails.Title == "Resource Not Found" &&
                ctx.ProblemDetails.Detail == "Entity with ID 123 not found"
            )
        ), Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_UnexpectedException_Returns500WithInternalServerError()
    {
        // Arrange
        var ex = new NotImplementedException("Feature not implemented");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, ex, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, _httpContext.Response.StatusCode);

        _mockProblemDetailsService.Verify(x => x.WriteAsync(
            It.Is<ProblemDetailsContext>(ctx =>
                ctx.ProblemDetails.Title == "Internal Server Error" &&
                ctx.ProblemDetails.Detail == "An unexpected error occurred. Please try again later." &&
                ctx.Exception == ex // 5xx includes exception
            )
        ), Times.Once);

        // Verify error logging for 5xx
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                ex,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_SetsCorrectHttpRequestPath()
    {
        // Arrange
        _httpContext.Request.Path = "/api/teams/123";
        var ex = new InvalidOperationException("Test exception");

        // Act
        await _handler.TryHandleAsync(_httpContext, ex, CancellationToken.None);

        // Assert
        _mockProblemDetailsService.Verify(x => x.WriteAsync(
            It.Is<ProblemDetailsContext>(ctx =>
                ctx.ProblemDetails.Instance == "/api/teams/123"
            )
        ), Times.Once);
    }
}
