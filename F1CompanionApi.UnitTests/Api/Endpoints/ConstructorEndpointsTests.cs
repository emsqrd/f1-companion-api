using F1CompanionApi.Api.Endpoints;
using F1CompanionApi.Api.Models;
using F1CompanionApi.Domain.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;

namespace F1CompanionApi.UnitTests.Api.Endpoints;

public class ConstructorEndpointsTests
{
    private readonly Mock<ILogger> _mockLogger;
    private readonly Mock<IConstructorService> _mockConstructorService;

    public ConstructorEndpointsTests()
    {
        _mockLogger = new Mock<ILogger>();
        _mockConstructorService = new Mock<IConstructorService>();
    }

    [Fact]
    public async Task GetConstructorsAsync_ReturnsOkWithConstructors()
    {
        // Arrange
        var constructors = new List<ConstructorResponse>
        {
            new ConstructorResponse
            {
                Id = 1,
                Type = "constructor",
                Name = "McLaren",
                FullName = "McLaren F1 Team",
                CountryAbbreviation = "GBR",
                IsActive = true
            },
            new ConstructorResponse
            {
                Id = 2,
                Type = "constructor",
                Name = "Ferrari",
                FullName = "Scuderia Ferrari",
                CountryAbbreviation = "ITA",
                IsActive = true
            }
        };

        _mockConstructorService.Setup(x => x.GetConstructorsAsync(null))
            .ReturnsAsync(constructors);

        // Act
        var result = await InvokeGetConstructorsAsync(null);

        // Assert
        Assert.IsType<Ok<IEnumerable<ConstructorResponse>>>(result);
        var okResult = (Ok<IEnumerable<ConstructorResponse>>)result;
        Assert.Equal(2, okResult.Value!.Count());
    }

    [Fact]
    public async Task GetConstructorsAsync_WithActiveOnlyTrue_ReturnsOnlyActiveConstructors()
    {
        // Arrange
        var constructors = new List<ConstructorResponse>
        {
            new ConstructorResponse
            {
                Id = 1,
                Type = "constructor",
                Name = "McLaren",
                FullName = "McLaren F1 Team",
                CountryAbbreviation = "GBR",
                IsActive = true
            }
        };

        _mockConstructorService.Setup(x => x.GetConstructorsAsync(true))
            .ReturnsAsync(constructors);

        // Act
        var result = await InvokeGetConstructorsAsync(true);

        // Assert
        Assert.IsType<Ok<IEnumerable<ConstructorResponse>>>(result);
        var okResult = (Ok<IEnumerable<ConstructorResponse>>)result;
        Assert.Single(okResult.Value!);
        Assert.True(okResult.Value!.First().IsActive);
    }

    [Fact]
    public async Task GetConstructorByIdAsync_ExistingConstructor_ReturnsOk()
    {
        // Arrange
        var constructor = new ConstructorResponse
        {
            Id = 1,
            Type = "constructor",
            Name = "McLaren",
            FullName = "McLaren F1 Team",
            CountryAbbreviation = "GBR",
            IsActive = true
        };

        _mockConstructorService.Setup(x => x.GetConstructorByIdAsync(1))
            .ReturnsAsync(constructor);

        // Act
        var result = await InvokeGetConstructorByIdAsync(1);

        // Assert
        Assert.IsType<Ok<ConstructorResponse>>(result);
        var okResult = (Ok<ConstructorResponse>)result;
        Assert.Equal(1, okResult.Value!.Id);
        Assert.Equal("McLaren", okResult.Value!.Name);
        Assert.Equal("McLaren F1 Team", okResult.Value!.FullName);
    }

    [Fact]
    public async Task GetConstructorByIdAsync_NonExistentConstructor_ReturnsProblem()
    {
        // Arrange
        _mockConstructorService.Setup(x => x.GetConstructorByIdAsync(999))
            .ReturnsAsync((ConstructorResponse?)null);

        // Act
        var result = await InvokeGetConstructorByIdAsync(999);

        // Assert
        Assert.IsType<ProblemHttpResult>(result);
        var problemResult = (ProblemHttpResult)result;
        Assert.Equal(StatusCodes.Status404NotFound, problemResult.StatusCode);
    }

    private async Task<IResult> InvokeGetConstructorsAsync(bool? activeOnly)
    {
        var method = typeof(ConstructorEndpoints).GetMethod(
            "GetConstructorsAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
        );

        var task = (Task<IResult>)method!.Invoke(
            null,
            new object?[] { _mockConstructorService.Object, activeOnly, _mockLogger.Object }
        )!;

        return await task;
    }

    private async Task<IResult> InvokeGetConstructorByIdAsync(int id)
    {
        var method = typeof(ConstructorEndpoints).GetMethod(
            "GetConstructorByIdAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
        );

        var task = (Task<IResult>)method!.Invoke(
            null,
            new object[] { _mockConstructorService.Object, id, _mockLogger.Object }
        )!;

        return await task;
    }
}
