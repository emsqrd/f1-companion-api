# F1 Fantasy Sports API - AI Coding Agent Instructions

## Architecture Overview

This is a .NET 9 Minimal API application for F1 fantasy sports with Supabase authentication and PostgreSQL database. The API follows a clean architecture with clear separation between endpoints, business logic (services), and data access.

### Key Architectural Patterns

- **Minimal API Endpoints**: Uses ASP.NET Core Minimal APIs with endpoint mapping organized in static classes under `Api/Endpoints/`. Endpoints are grouped under `/api` prefix and use extension methods for registration
- **Service Layer**: Business logic encapsulated in service classes under `Domain/Services/`. Services implement interfaces for dependency injection and testability. All services are registered in `ServiceExtensions.cs`
- **Data Access**: Entity Framework Core with PostgreSQL provider. `ApplicationDbContext` manages entities with navigation properties and relationships. In-memory database used for unit tests
- **Authentication**: JWT Bearer authentication with Supabase. Tokens validated against Supabase JWT secret. Protected endpoints require `RequireAuthorization()` in endpoint configuration
- **Entity Pattern**: All entities inherit from `BaseEntity` which provides audit trail fields (`CreatedBy`, `CreatedAt`, `UpdatedBy`, `UpdatedAt`, `DeletedBy`, `DeletedAt`) with corresponding navigation properties to `UserProfile`

### Key Technologies

- **.NET 9** - latest .NET with C# 13 and nullable reference types enabled
- **ASP.NET Core Minimal APIs** - lightweight endpoint routing pattern
- **Entity Framework Core 9** with PostgreSQL - ORM and database provider
- **Supabase** - authentication and user management
- **Sentry** - error tracking, performance monitoring, and structured logging
- **xUnit** - test framework with Fact/Theory attributes
- **Moq** - mocking framework for unit tests
- **Coverlet** - code coverage collection

### Specialized Guidelines

For detailed guidance on specific topics, refer to these specialized instruction files:

- **[Sentry Integration](instructions/sentry.md)** - Error tracking, performance monitoring, and structured logging best practices

## Development Workflow

### Essential Commands

```bash
dotnet run --project F1CompanionApi/F1CompanionApi.csproj    # Run API locally
dotnet watch run --project F1CompanionApi/F1CompanionApi.csproj  # Run with hot reload
dotnet build F1CompanionApi/F1CompanionApi.csproj            # Build project
dotnet test F1CompanionApi.UnitTests/F1CompanionApi.UnitTests.csproj  # Run tests
./run-coverage.sh                                             # Generate coverage report
./run-coverage.sh --open                                      # Generate and open coverage report
```

### Testing Conventions

- **xUnit + Moq**: All services and endpoints have test files in `F1CompanionApi.UnitTests/` mirroring source structure
- **Naming Convention**: Test classes named `{ClassName}Tests.cs` (e.g., `LeagueServiceTests.cs`, `LeagueEndpointsTests.cs`)
- **Test Method Pattern**: `{MethodName}_{Scenario}_{ExpectedOutcome}` (e.g., `CreateLeagueAsync_ValidRequest_ReturnsLeagueResponseWithCorrectData`)
- **In-Memory Database**: Service tests use `UseInMemoryDatabase()` with unique GUID-based database names to ensure test isolation
- **Reflection for Endpoint Tests**: Endpoint tests use reflection to invoke private static endpoint methods since Minimal API endpoints are private by design
- **Coverage Exclusions**: Excludes `Program.cs`, `Migrations/`, `Api/Models/`, `Data/Entities/`, and `ServiceExtensions.cs` via project file configuration

**Example Test Generation Prompt:**

```
Generate high-value tests for this service/endpoint following our testing philosophy.
Focus on business logic only - do not test EF Core behavior, .NET framework internals,
or language features. Keep it lean (~10-15 tests).
```

## Endpoint Patterns

### Minimal API Structure

- **Organization**: Endpoint classes in `Api/Endpoints/` as static classes with `Map{Feature}Endpoints` extension methods
- **Registration**: Central `Endpoints.MapEndpoints()` method chains all feature endpoint mappings
- **Configuration**: Each endpoint configured with `.RequireAuthorization()`, `.WithName()`, `.WithOpenApi()`, `.WithDescription()`
- **Route Parameters**: Use method parameters for route values, query strings, and request bodies
- **Dependency Injection**: Services injected directly as method parameters
- **Response Pattern**: Return `IResult` using `Results.Ok()`, `Results.Created()`, `Results.NotFound()`, etc.

### Example Endpoint Pattern

```csharp
public static class FeatureEndpoints
{
    public static IEndpointRouteBuilder MapFeatureEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/feature/{id}", GetByIdAsync)
            .RequireAuthorization()
            .WithName("GetFeatureById")
            .WithOpenApi()
            .WithDescription("Get feature by ID");

        return app;
    }

    private static async Task<IResult> GetByIdAsync(
        int id,
        IFeatureService service
    )
    {
        var result = await service.GetByIdAsync(id);

        if (result is null)
        {
            return Results.NotFound("Feature not found");
        }

        return Results.Ok(result);
    }
}
```

## Service Patterns

### Service Layer Architecture

- **Interface-Based**: All services implement interfaces (e.g., `ILeagueService`, `IUserProfileService`)
- **Constructor Injection**: Services receive dependencies (typically `ApplicationDbContext`) via constructor
- **Null Validation**: Constructor parameters validated with `ArgumentNullException`
- **Async Operations**: All database operations use async/await pattern with `Task<T>` return types
- **EF Core Includes**: Use `.Include()` for eager loading navigation properties to avoid lazy loading issues
- **Business Validation**: Services throw domain exceptions (e.g., `InvalidOperationException`) for business rule violations

### Example Service Pattern

```csharp
public interface IFeatureService
{
    Task<FeatureResponse> CreateAsync(CreateFeatureRequest request, int userId);
    Task<IEnumerable<Feature>> GetAllAsync();
    Task<Feature?> GetByIdAsync(int id);
}

public class FeatureService : IFeatureService
{
    private readonly ApplicationDbContext _dbContext;

    public FeatureService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<Feature?> GetByIdAsync(int id)
    {
        return await _dbContext.Features
            .Include(x => x.RelatedEntity)
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}
```

## Entity & Data Patterns

### Entity Framework Core

- **Base Entity Pattern**: All entities inherit from `BaseEntity` for audit trail consistency
- **Navigation Properties**: Required navigation properties use `= null!;` with nullable reference types enabled
- **Fluent API Configuration**: Complex relationships configured in `ApplicationDbContext.OnModelCreating()`
- **Migrations**: Applied automatically on startup via `dbContext.Database.Migrate()` in `Program.cs`
- **Delete Behavior**: Related entities use `DeleteBehavior.Restrict` to prevent cascading deletes

### Common Entity Patterns

- **Leagues**: `{ Id, Name, Description, MaxTeams, IsPrivate, OwnerId, Owner }`
- **UserProfile**: `{ Id, AccountId, Email, FirstName, LastName, DisplayName, AvatarUrl }`
- **Audit Fields**: All entities have `CreatedBy`, `CreatedAt`, `UpdatedBy`, `UpdatedAt`, `DeletedBy`, `DeletedAt` from `BaseEntity`

## Configuration & Environment

### Required Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=f1companion;..."
  },
  "Supabase": {
    "JwtSecret": "your-jwt-secret"
  },
  "CorsOrigins": ["http://localhost:5173"]
}
```

### CORS Policy

- Configured in `ServiceExtensions.AddApplicationServices()`
- Supports exact origin matches and Netlify preview deployments (\*.netlify.app)
- Allows credentials, all headers, and all methods

## Documentation & Reference

### Microsoft Documentation MCP Server

When working with .NET/EF Core/ASP.NET Core features, consult official Microsoft documentation via MCP server when:

- Implementing new or unfamiliar framework features
- Working with EF Core migrations, relationships, or complex queries
- Configuring authentication/authorization patterns
- Using .NET 9 or C# 13 features you're uncertain about
- Troubleshooting framework-specific errors or unexpected behavior
- Need to verify PostgreSQL/Npgsql provider-specific behavior

**Tool Usage Pattern:**

1. Use `mcp_microsoftdocs_microsoft_docs_search` for initial discovery (e.g., "Entity Framework Core 9 relationship configuration")
2. Follow up with `mcp_microsoftdocs_microsoft_docs_fetch` when you need complete documentation from a specific page
3. Use `mcp_microsoftdocs_microsoft_code_sample_search` when you need practical code examples

**Skip the MCP server for:**

- Well-established patterns already implemented in the codebase
- Simple CRUD operations using existing service patterns
- Basic xUnit test structure (unless testing new framework features)

## Testing Strategy

### Testing Philosophy

**What to Test (High Value):**

- Business logic specific to your service/endpoint
- Data persistence and retrieval correctness
- Validation and error handling paths
- Service integration with database operations
- Endpoint request/response mapping
- Authorization and authentication workflows
- Exception propagation and error messages
- Edge cases (null inputs, missing data, etc.)

**What NOT to Test (Low Value):**

- Entity Framework Core query execution (LINQ to SQL translation)
- .NET framework internals (DI container, middleware pipeline)
- Language features (null-coalescing, pattern matching)
- Database provider behavior (PostgreSQL/In-Memory differences)
- Serialization/deserialization (JSON conversion)
- Static endpoint registration (MapEndpoints method structure)
- Configuration binding (appsettings.json mapping)

**Testing Approach:**

- Use in-memory database for service tests to verify business logic in isolation
- Mock services for endpoint tests to focus on endpoint behavior
- Test one success path and critical failure paths for each operation
- Verify returned data structure, not individual property assignments
- Focus on "what could break my business logic" not "what could break .NET"

### Service Testing

- **Test Class Setup**: Create in-memory context using `Guid.NewGuid().ToString()` for database name
- **Arrange**: Seed test data into context, create service instance
- **Act**: Call service method with test parameters
- **Assert**: Verify returned values and/or database state
- **Pattern**: `{MethodName}_{Scenario}_{ExpectedOutcome}` naming

### Endpoint Testing

- **Mock Dependencies**: Use Moq to create mock services and HttpContext
- **Reflection Access**: Use reflection to invoke private static endpoint methods
- **Helper Methods**: Create helper methods to reduce reflection boilerplate
- **Response Verification**: Assert on specific result types (`Ok<T>`, `Created<T>`, `NotFound<T>`)
- **Value Inspection**: Verify response payload structure and values

### Example Test Patterns

```csharp
// Service Test Pattern
[Fact]
public async Task GetByIdAsync_ExistingEntity_ReturnsEntity()
{
    // Arrange
    using var context = CreateInMemoryContext();
    var service = new FeatureService(context);

    var entity = new Feature { Name = "Test" };
    context.Features.Add(entity);
    await context.SaveChangesAsync();

    // Act
    var result = await service.GetByIdAsync(entity.Id);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("Test", result.Name);
}

// Endpoint Test Pattern
[Fact]
public async Task GetByIdAsync_ExistingEntity_ReturnsOk()
{
    // Arrange
    var entity = new Feature { Id = 1, Name = "Test" };
    _mockService.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(entity);

    // Act
    var result = await InvokeGetByIdAsync(1);

    // Assert
    Assert.IsType<Ok<Feature>>(result);
    var okResult = (Ok<Feature>)result;
    Assert.Equal(1, okResult.Value.Id);
}
```

### File Organization

- **Service Tests**: `F1CompanionApi.UnitTests/Services/{ServiceName}Tests.cs`
- **Endpoint Tests**: `F1CompanionApi.UnitTests/Api/Endpoints/{EndpointName}Tests.cs`
- **Extension Tests**: `F1CompanionApi.UnitTests/Extensions/{ExtensionName}Tests.cs`

## Best Practices

- **Nullable Reference Types**: Always enabled - use `?` for nullable types, `= null!;` for required navigation properties
- **Async/Await**: All I/O operations (database, HTTP) must be async
- **Constructor Validation**: Validate null dependencies with `ArgumentNullException`
- **Include Navigation Properties**: Always `.Include()` related entities before returning from services
- **Service Registration**: Register services in `ServiceExtensions.cs` with appropriate lifetime (Scoped for EF Core contexts and services)
- **Endpoint Documentation**: Use `.WithName()`, `.WithOpenApi()`, and `.WithDescription()` for all endpoints
- **Test Isolation**: Use unique in-memory database names per test to prevent cross-test contamination

When working on this codebase, prioritize separation of concerns, testability, and the established patterns for Minimal APIs, service layer, and Entity Framework Core usage.
