# Sentry Integration Guidelines for .NET API

## Overview

Sentry provides error tracking, performance monitoring, and structured logging for this .NET 9 ASP.NET Core Minimal API application. This integration follows best practices from both Sentry and Microsoft documentation to ensure proper implementation and consistent patterns across the codebase.

## Setup & Configuration

### Package Installation

The project uses the `Sentry.AspNetCore` NuGet package (v5.16.2+), which includes:

- Core Sentry SDK for .NET
- ASP.NET Core-specific integrations
- Structured logging support via `Sentry.Extensions.Logging`

### Configuration Structure

Sentry is configured via `appsettings.json` with the following key settings:

```json
{
  "Sentry": {
    "Dsn": "https://your-dsn@sentry.io/project-id",
    "SendDefaultPii": true,
    "MaxRequestBodySize": "Always",
    "MinimumBreadcrumbLevel": "Debug",
    "MinimumEventLevel": "Warning",
    "AttachStackTrace": true,
    "Debug": false,
    "DiagnosticLevel": "Error",
    "TracesSampleRate": 1.0,
    "Environment": "Development",
    "Experimental": {
      "EnableLogs": true
    }
  }
}
```

#### Configuration Properties Explained

- **Dsn**: Data Source Name - unique identifier for your Sentry project
- **SendDefaultPii**: Captures request URL, headers, IP addresses, and user information
- **MaxRequestBodySize**: Controls how much of the request body to capture (Always/Medium/Small/None)
- **MinimumBreadcrumbLevel**: Lowest log level for breadcrumbs (contextual information)
- **MinimumEventLevel**: Lowest log level to send as events to Sentry
- **AttachStackTrace**: Includes stack traces for all events
- **Debug**: Enables SDK debug output (disable in production)
- **DiagnosticLevel**: Controls internal SDK diagnostic logging
- **TracesSampleRate**: Percentage of transactions to send for performance monitoring (0.0 to 1.0)
- **Environment**: Environment name (Development/Staging/Production)
- **Experimental.EnableLogs**: Enables structured logging capture

### Initialization

Sentry is initialized early in `Program.cs` before building the application:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseSentry(options =>
{
    // Load configuration from appsettings.json
    options.Dsn = builder.Configuration["Sentry:Dsn"];
    options.Debug = builder.Configuration.GetValue<bool>("Sentry:Debug");
    options.Environment = builder.Configuration["Sentry:Environment"]
        ?? builder.Environment.EnvironmentName;

    // Enable structured logging
    options.Experimental.EnableLogs = builder.Configuration
        .GetValue<bool>("Sentry:Experimental:EnableLogs");

    // Performance monitoring
    options.TracesSampleRate = builder.Configuration
        .GetValue<double>("Sentry:TracesSampleRate");

    // Privacy settings
    options.SendDefaultPii = builder.Configuration
        .GetValue<bool>("Sentry:SendDefaultPii");
    var maxBodySize = builder.Configuration["Sentry:MaxRequestBodySize"];
    if (maxBodySize == "Always")
    {
        options.MaxRequestBodySize = Sentry.Extensibility.RequestSize.Always;
    }

    // Event filtering
    options.SetBeforeSend((@event, hint) =>
    {
        // Remove server names for privacy
        @event.ServerName = null;
        return @event;
    });

    // Log filtering
    options.Experimental.SetBeforeSendLog(log =>
    {
        // Filter debug/trace logs in production
        if (!builder.Environment.IsDevelopment() &&
            (log.Level is Sentry.SentryLogLevel.Debug ||
             log.Level is Sentry.SentryLogLevel.Trace))
        {
            return null;
        }
        return log;
    });
});
```

## Structured Logging

### Using ILogger (Recommended)

The primary way to log in this application is through `ILogger<T>` dependency injection, which automatically integrates with Sentry:

```csharp
public class LeagueService : ILeagueService
{
    private readonly ILogger<LeagueService> _logger;

    public LeagueService(ILogger<LeagueService> logger)
    {
        _logger = logger;
    }

    public async Task<League?> GetLeagueAsync(int id)
    {
        _logger.LogInformation("Fetching league {LeagueId}", id);

        try
        {
            var league = await _dbContext.Leagues.FindAsync(id);

            if (league == null)
            {
                _logger.LogWarning("League {LeagueId} not found", id);
            }

            return league;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch league {LeagueId}", id);
            throw;
        }
    }
}
```

### Log Levels Mapping

Sentry automatically maps .NET log levels to Sentry severity levels:

| .NET LogLevel | Sentry Level | Use Case                                     |
| ------------- | ------------ | -------------------------------------------- |
| `Trace`       | Debug        | Very detailed diagnostic information         |
| `Debug`       | Debug        | Debugging information for development        |
| `Information` | Info         | General informational messages               |
| `Warning`     | Warning      | Potential issues that should be reviewed     |
| `Error`       | Error        | Errors that need attention                   |
| `Critical`    | Fatal        | Critical failures requiring immediate action |

### Structured Logging Best Practices

#### ✅ Do's

```csharp
// Good - Use named placeholders for structured data
_logger.LogInformation("User {UserId} created league {LeagueId}", userId, leagueId);

// Good - Include contextual data
_logger.LogWarning("Rate limit approaching for endpoint {Endpoint}. Remaining: {Remaining}",
    endpoint, remainingCalls);

// Good - Log exceptions with context
_logger.LogError(exception, "Payment processing failed for order {OrderId}", orderId);

// Good - Use appropriate log levels
_logger.LogDebug("Cache hit for key {CacheKey}", key);
_logger.LogInformation("Team {TeamId} submitted successfully", teamId);
_logger.LogWarning("Slow query detected for {Operation}", operationName);
_logger.LogError(ex, "Database connection failed");
```

#### ❌ Don'ts

```csharp
// Bad - String concatenation loses structure
_logger.LogInformation("User " + userId + " created league " + leagueId);

// Bad - String interpolation loses structure
_logger.LogInformation($"User {userId} created league {leagueId}");

// Bad - Logging sensitive information
_logger.LogInformation("User password: {Password}", password); // Never log passwords!
_logger.LogDebug("Credit card: {CardNumber}", cardNumber);     // Never log PII!

// Bad - Excessive logging
_logger.LogDebug("Entering method"); // Adds noise
_logger.LogDebug("Exiting method");  // Not useful

// Bad - Wrong log level
_logger.LogError("User logged in successfully"); // Use Information instead
```

### Adding Custom Attributes

When you need additional context beyond the log message:

```csharp
// Using EventId for categorization
_logger.LogWarning(
    new EventId(1001, "RateLimitWarning"),
    "Rate limit approaching for {Endpoint}",
    endpoint
);

// The EventId is automatically captured as attributes in Sentry
```

## Error Tracking

### Automatic Error Capture

ASP.NET Core exceptions are automatically captured by Sentry. No additional code needed for:

- Unhandled exceptions in endpoints
- Middleware exceptions
- Background service exceptions

### Manual Exception Capture

For caught exceptions that you want to track:

```csharp
using Sentry;

public async Task ProcessPaymentAsync(Order order)
{
    try
    {
        await _paymentGateway.ChargeAsync(order);
    }
    catch (PaymentException ex)
    {
        // Log via ILogger (recommended - automatically sends to Sentry)
        _logger.LogError(ex, "Payment failed for order {OrderId}", order.Id);

        // OR use SentrySdk directly for additional context
        SentrySdk.CaptureException(ex, scope =>
        {
            scope.SetTag("payment_gateway", "stripe");
            scope.SetExtra("order_amount", order.Amount);
            scope.SetExtra("order_currency", order.Currency);
        });

        // Handle gracefully for user
        throw new UserFriendlyException("Payment processing failed");
    }
}
```

### When to Use SentrySdk.CaptureException

Only use `SentrySdk.CaptureException` when you need:

- Additional tags or context beyond what ILogger provides
- To capture exceptions without logging them
- Fine-grained control over scope and context

**For most cases, use ILogger.LogError() instead.**

## Performance Monitoring

### Automatic Instrumentation

Sentry automatically captures performance data for:

- HTTP requests (via ASP.NET Core middleware)
- Database queries (via Entity Framework Core)
- Outgoing HTTP calls (if `HttpClient` integration is enabled)

### Transaction Naming

Transactions are automatically named using the pattern: `<HTTP Method> <Route>`

Examples:

- `GET /api/leagues`
- `POST /api/teams`
- `PUT /api/leagues/{id}`

### Custom Performance Tracking

For specific operations you want to track:

```csharp
using Sentry;

public async Task<Report> GenerateReportAsync(int leagueId)
{
    var transaction = SentrySdk.StartTransaction(
        "report.generation",
        $"Generate Report for League {leagueId}"
    );

    try
    {
        // Track specific spans
        var dbSpan = transaction.StartChild("db.query", "Fetch league data");
        var data = await _dbContext.Leagues
            .Include(l => l.Teams)
            .FirstOrDefaultAsync(l => l.Id == leagueId);
        dbSpan.Finish();

        var processingSpan = transaction.StartChild("processing", "Calculate statistics");
        var report = ProcessData(data);
        processingSpan.Finish();

        transaction.Status = SpanStatus.Ok;
        return report;
    }
    catch (Exception ex)
    {
        transaction.Status = SpanStatus.InternalError;
        _logger.LogError(ex, "Report generation failed for league {LeagueId}", leagueId);
        throw;
    }
    finally
    {
        transaction.Finish();
    }
}
```

## Integration with Application Architecture

### Service Layer Pattern

All services in `Domain/Services/` should inject `ILogger<TService>`:

```csharp
public interface ILeagueService
{
    Task<LeagueResponse> CreateLeagueAsync(CreateLeagueRequest request, int userId);
}

public class LeagueService : ILeagueService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<LeagueService> _logger;

    public LeagueService(
        ApplicationDbContext dbContext,
        ILogger<LeagueService> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<LeagueResponse> CreateLeagueAsync(
        CreateLeagueRequest request,
        int userId)
    {
        _logger.LogInformation(
            "Creating league {LeagueName} for user {UserId}",
            request.Name,
            userId
        );

        // Implementation...
    }
}
```

### Endpoint Pattern

Minimal API endpoints can use ILogger via dependency injection:

```csharp
public static class LeagueEndpoints
{
    private static async Task<IResult> CreateLeagueAsync(
        CreateLeagueRequest request,
        ILeagueService service,
        ILogger<LeagueEndpoints> logger,
        HttpContext context)
    {
        var userId = context.GetUserId();

        logger.LogInformation(
            "Endpoint: Creating league for user {UserId}",
            userId
        );

        try
        {
            var response = await service.CreateLeagueAsync(request, userId);
            return Results.Created($"/api/leagues/{response.Id}", response);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Invalid league creation request");
            return Results.BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error creating league");
            return Results.Problem("An error occurred while creating the league");
        }
    }
}
```

## Environment-Specific Configuration

### Development

```json
{
  "Sentry": {
    "Debug": true,
    "TracesSampleRate": 1.0,
    "MinimumEventLevel": "Information",
    "Environment": "Development"
  }
}
```

### Staging

```json
{
  "Sentry": {
    "Debug": false,
    "TracesSampleRate": 0.5,
    "MinimumEventLevel": "Warning",
    "Environment": "Staging"
  }
}
```

### Production

```json
{
  "Sentry": {
    "Debug": false,
    "TracesSampleRate": 0.1,
    "MinimumEventLevel": "Error",
    "Environment": "Production",
    "DiagnosticLevel": "Error"
  }
}
```

## Testing Considerations

### Unit Tests

Mock `ILogger<T>` in unit tests:

```csharp
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class LeagueServiceTests
{
    private readonly Mock<ILogger<LeagueService>> _mockLogger;

    public LeagueServiceTests()
    {
        _mockLogger = new Mock<ILogger<LeagueService>>();
    }

    [Fact]
    public async Task CreateLeagueAsync_LogsInformation()
    {
        // Arrange
        var service = new LeagueService(_dbContext, _mockLogger.Object);

        // Act
        await service.CreateLeagueAsync(request, userId);

        // Assert - verify logging behavior if critical
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }
}
```

### Integration Tests

Sentry can be disabled in test environments by omitting the DSN configuration.

## Best Practices Summary

### Logging

1. **Always use ILogger<T>** for logging in services and endpoints
2. **Use structured logging** with named placeholders, not string interpolation
3. **Choose appropriate log levels** - Info for normal operations, Warning for issues, Error for failures
4. **Never log sensitive data** - passwords, tokens, credit cards, SSNs, etc.
5. **Include context** - user IDs, entity IDs, operation names
6. **Log exceptions with context** - use `LogError(exception, message, ...args)`

### Performance Monitoring

1. **Set appropriate sample rates** - 1.0 (100%) for dev, 0.1 (10%) for production
2. **Monitor trace volume** - high traffic apps should use lower sample rates
3. **Use custom transactions** for critical business operations
4. **Name transactions descriptively** - helps identify slow operations

### Error Tracking

1. **Let ASP.NET Core handle most errors** - automatic capture works well
2. **Use ILogger for caught exceptions** - simpler than SentrySdk
3. **Add context when capturing** - helps debugging and triage
4. **Filter noise** - use BeforeSend callbacks to exclude non-actionable errors

### Privacy & Security

1. **Never log PII** without user consent
2. **Scrub sensitive data** in BeforeSend callbacks
3. **Use SendDefaultPii cautiously** - understand what data is sent
4. **Remove server names** - prevents infrastructure exposure
5. **Configure MaxRequestBodySize** appropriately for your data sensitivity

## Troubleshooting

### Logs Not Appearing in Sentry

1. Verify DSN is set correctly in configuration
2. Check that `Experimental.EnableLogs` is `true`
3. Verify log level meets `MinimumEventLevel` threshold
4. Check `BeforeSendLog` callback isn't filtering the log
5. Ensure Sentry is initialized before logging occurs

### High Event Volume

1. Increase `MinimumEventLevel` to `Warning` or `Error`
2. Reduce `TracesSampleRate` to a lower percentage
3. Implement filtering in `BeforeSend` and `BeforeSendLog` callbacks
4. Review excessive logging in application code

### Performance Impact

1. Use async logging (Sentry does this by default)
2. Reduce sample rates in high-traffic scenarios
3. Disable `Debug` mode in production
4. Monitor SDK diagnostic output for issues

## Resources

- [Sentry .NET SDK Documentation](https://docs.sentry.io/platforms/dotnet/)
- [Sentry ASP.NET Core Guide](https://docs.sentry.io/platforms/dotnet/guides/aspnetcore/)
- [Microsoft Logging Documentation](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/)
- [Structured Logging Best Practices](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging)

## Support

For issues or questions about Sentry integration:

1. Check this documentation first
2. Review Sentry documentation and Microsoft docs
3. Check application logs for Sentry diagnostic messages
4. Contact team lead or DevOps for production issues
