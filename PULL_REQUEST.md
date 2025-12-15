# Phase 2: Custom Domain Exceptions Implementation

## 🎯 Overview

This PR implements strongly-typed custom domain exceptions to replace generic `InvalidOperationException` usage across the API. This improves error handling, debugging, and API consumer experience by providing semantic exception types with rich context.

**Issue:** Resolves #11 (Custom Domain Exceptions)

## ✨ Key Features

### 1. **7 Custom Domain Exception Classes** (`F1CompanionApi/Domain/Exceptions/`)

All exceptions inherit from `Exception` and provide structured context for specific business rule violations:

| Exception | HTTP Status | Use Case | Properties |
|---|---|---|---|
| `UserProfileNotFoundException` | 400 Bad Request | User profile not found for authenticated account | `AccountId`, `UserId` |
| `TeamOwnershipException` | 403 Forbidden | User attempting to modify another user's team | `TeamId`, `OwnerId`, `AttemptedUserId` |
| `DuplicateTeamException` | 409 Conflict | User attempting to create multiple teams | `UserId`, `ExistingTeamId` |
| `SlotOccupiedException` | 409 Conflict | Attempting to add entity to occupied slot | `Position`, `TeamId` |
| `EntityAlreadyOnTeamException` | 409 Conflict | Attempting to add entity that's already on team | `EntityId`, `EntityType`, `TeamId` |
| `TeamFullException` | 400 Bad Request | Team has reached maximum capacity | `TeamId`, `MaxSlots`, `EntityType` |
| `InvalidSlotPositionException` | 400 Bad Request | Invalid slot position for entity type | `Position`, `MaxPosition`, `EntityType` |

### 2. **GlobalExceptionHandler Integration** (`F1CompanionApi/Domain/Exceptions/GlobalExceptionHandler.cs`)

Enhanced exception handler that:
- Maps all 7 custom exceptions to appropriate HTTP status codes
- Provides user-friendly error messages via RFC 7807 Problem Details
- Maintains existing PostgreSQL error handling
- Logs at appropriate levels (Warning for 4xx, Error for 5xx)
- Excludes exception details from 4xx responses (security best practice)
- Includes exception details in 5xx responses (debugging support)

### 3. **Service Layer Refactoring** (5 Services Updated)

**TeamService** (`Domain/Services/TeamService.cs`):
- `CreateTeamAsync`: Throws `UserProfileNotFoundException`, `DuplicateTeamException`
- `AddDriverToTeamAsync`: Throws `TeamOwnershipException`, `InvalidSlotPositionException`, `TeamFullException`, `SlotOccupiedException`, `EntityAlreadyOnTeamException`
- `AddConstructorToTeamAsync`: Same exception types as drivers
- `RemoveDriverFromTeamAsync`: Throws `TeamOwnershipException`
- `RemoveConstructorFromTeamAsync`: Throws `TeamOwnershipException`

**UserProfileService** (`Domain/Services/UserProfileService.cs`):
- `GetRequiredCurrentUserProfileAsync`: Throws `UserProfileNotFoundException`

**LeagueService** (`Domain/Services/LeagueService.cs`):
- `CreateLeagueAsync`: Throws `UserProfileNotFoundException`

**SupabaseAuthService** (No changes):
- Infrastructure exceptions remain as `InvalidOperationException` (correct pattern)

### 4. **Comprehensive Test Coverage** (150 Tests, 100% Passing)

**Exception Unit Tests** (14 tests):
- Each exception has 2 focused tests:
  - `Constructor_SetsAllPropertiesCorrectly` - validates property assignment
  - `Constructor_FormatsMessageWithCriticalContext` - validates message includes debugging context

**GlobalExceptionHandler Tests** (21 tests):
- 7 PostgreSQL error mapping tests (Theory-based parameterization)
- 7 custom domain exception tests
- 5 generic exception handling tests
- 1 HTTP context test
- 1 unexpected exception test

**Service Tests** (115 tests):
- All service tests updated to expect custom exception types
- Property assertions validate exception context
- Message assertions validate debugging information

## 🔧 Technical Details

### Exception Design Patterns

1. **Immutable Properties**: All exception properties are `init`-only for thread safety
2. **Rich Context**: Each exception captures IDs and contextual data for debugging
3. **Descriptive Messages**: Messages include all critical information for logs and troubleshooting
4. **Nullable Support**: Properly handles nullable reference types (C# 13)

### Test Quality Standards

- ✅ Follows xUnit best practices (Fact/Theory attributes, AAA pattern)
- ✅ Tests behavior, not implementation
- ✅ High-value assertions only (no testing framework internals)
- ✅ Theory tests for parameterized scenarios (PostgreSQL errors)
- ✅ Focused, single-purpose test methods

### Sentry Integration

All custom exceptions integrate with existing Sentry error tracking:
- Structured logging via `ILogger` in services
- Automatic exception capture by GlobalExceptionHandler
- Rich context data for debugging (user IDs, entity IDs, team IDs)

## 📊 Test Results

```bash
Test Run Successful.
Total tests: 150
     Passed: 150
 Total time: 2.9s
```

**Coverage Report**: `coverage/report/index.html` (comprehensive coverage with exclusions for migrations, entities, and DTOs)

## 🚨 Breaking Changes

### For API Consumers

**None** - All changes are internal. HTTP status codes and Problem Details responses remain consistent.

### For Internal Development

**Service Method Signatures** - No changes to public interfaces
**Exception Types** - Services now throw typed exceptions instead of `InvalidOperationException`

**Migration Guide for Future Development:**
```csharp
// Before
throw new InvalidOperationException("User not found");

// After
throw new UserProfileNotFoundException(accountId);
```

## 📝 Files Changed

### New Files (7)
- `F1CompanionApi/Domain/Exceptions/UserProfileNotFoundException.cs`
- `F1CompanionApi/Domain/Exceptions/TeamOwnershipException.cs`
- `F1CompanionApi/Domain/Exceptions/DuplicateTeamException.cs`
- `F1CompanionApi/Domain/Exceptions/SlotOccupiedException.cs`
- `F1CompanionApi/Domain/Exceptions/EntityAlreadyOnTeamException.cs`
- `F1CompanionApi/Domain/Exceptions/TeamFullException.cs`
- `F1CompanionApi/Domain/Exceptions/InvalidSlotPositionException.cs`

### Modified Files (10)
- `F1CompanionApi/Domain/Exceptions/GlobalExceptionHandler.cs` - Added custom exception cases
- `F1CompanionApi/Domain/Services/TeamService.cs` - Updated to throw typed exceptions
- `F1CompanionApi/Domain/Services/UserProfileService.cs` - Updated to throw typed exceptions
- `F1CompanionApi/Domain/Services/LeagueService.cs` - Updated to throw typed exceptions
- `F1CompanionApi.UnitTests/Domain/Exceptions/GlobalExceptionHandlerTests.cs` - Added 7 new tests
- `F1CompanionApi.UnitTests/Services/TeamServiceTests.cs` - Updated 18 tests
- `F1CompanionApi.UnitTests/Services/UserProfileServiceTests.cs` - Updated 1 test
- `F1CompanionApi.UnitTests/Services/LeagueServiceTests.cs` - Updated 1 test
- `F1CompanionApi.UnitTests/Domain/Exceptions/*ExceptionTests.cs` (7 files) - New test files

## ✅ Checklist

- [x] All tests passing (150/150)
- [x] Code coverage verified
- [x] xUnit best practices followed
- [x] Exception messages include debugging context
- [x] GlobalExceptionHandler maps all custom exceptions
- [x] Sentry integration validated
- [x] No breaking changes to public APIs
- [x] Service layer refactoring complete
- [x] Infrastructure exceptions remain unchanged (SupabaseAuthService)

## 🔍 Code Review Focus Areas

1. **Exception Message Quality** - Verify messages provide actionable debugging information
2. **Property Immutability** - Confirm all exception properties use `init` accessors
3. **Test Coverage** - Review that all exception paths are tested
4. **GlobalExceptionHandler Logic** - Validate mapping logic for each exception type
5. **Logging Levels** - Confirm Warning for 4xx, Error for 5xx

## 📚 Related Documentation

- [Sentry Integration Guide](docs/sentry-setup.md)
- [Testing Guidelines](.github/instructions/testing.md)
- [Architecture Guidelines](.github/copilot-instructions.md)

## 🚀 Next Steps

After merge:
1. Monitor Sentry for custom exception capture
2. Review production error patterns
3. Consider additional domain exceptions as patterns emerge
4. Update error handling documentation if needed

---

**Review Requested From**: @team-leads
**Estimated Review Time**: 30-45 minutes
**Priority**: Medium
**Target Merge**: Sprint 2025-Q4
