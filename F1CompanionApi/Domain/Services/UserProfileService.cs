using F1CompanionApi.Api.Models;
using F1CompanionApi.Data;
using F1CompanionApi.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace F1CompanionApi.Domain.Services;

public interface IUserProfileService
{
    Task<UserProfile?> GetUserProfileByAccountIdAsync(string accountId);
    Task<UserProfile?> GetCurrentUserProfileAsync();
    Task<UserProfile> GetRequiredCurrentUserProfileAsync();
    Task<UserProfile> CreateUserProfileAsync(
        string accountId,
        string email,
        string? displayName = null
    );
    Task<UserProfileResponse> UpdateUserProfileAsync(
        UpdateUserProfileRequest updateUserProfileRequest
    );
}

public class UserProfileService : IUserProfileService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ISupabaseAuthService _authService;

    public UserProfileService(ApplicationDbContext dbContext, ISupabaseAuthService authService)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    public async Task<UserProfile?> GetUserProfileByAccountIdAsync(string accountId)
    {
        return await _dbContext
            .UserProfiles.Include(x => x.Account)
            .FirstOrDefaultAsync(x => x.AccountId == accountId);
    }

    public async Task<UserProfile?> GetCurrentUserProfileAsync()
    {
        var userId = _authService.GetUserId();
        if (userId is null)
        {
            return null;
        }

        return await GetUserProfileByAccountIdAsync(userId);
    }

    public async Task<UserProfile> GetRequiredCurrentUserProfileAsync()
    {
        var userId = _authService.GetRequiredUserId();
        return await GetUserProfileByAccountIdAsync(userId)
            ?? throw new InvalidOperationException("User profile not found for authenticated user");
    }

    public async Task<UserProfile> CreateUserProfileAsync(
        string accountId,
        string email,
        string? displayName = null
    )
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            // Create Account
            var account = new Account
            {
                Id = accountId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                LastLoginAt = DateTime.UtcNow,
            };

            _dbContext.Accounts.Add(account);

            // Create UserProfile
            var userProfile = new UserProfile
            {
                AccountId = accountId,
                Email = email,
                DisplayName = displayName,
            };

            _dbContext.UserProfiles.Add(userProfile);

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return userProfile;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<UserProfileResponse> UpdateUserProfileAsync(
        UpdateUserProfileRequest updateUserProfileRequest
    )
    {
        var existingUserProfile = await _dbContext.UserProfiles.FindAsync(
            updateUserProfileRequest.Id
        );

        if (existingUserProfile is null)
            throw new KeyNotFoundException($"User with ID {updateUserProfileRequest.Id} not found");

        if (updateUserProfileRequest.DisplayName is not null)
            existingUserProfile.DisplayName = updateUserProfileRequest.DisplayName;

        if (updateUserProfileRequest.Email is not null)
            existingUserProfile.Email = updateUserProfileRequest.Email;

        if (updateUserProfileRequest.FirstName is not null)
            existingUserProfile.FirstName = updateUserProfileRequest.FirstName;

        if (updateUserProfileRequest.LastName is not null)
            existingUserProfile.LastName = updateUserProfileRequest.LastName;

        if (updateUserProfileRequest.AvatarUrl is not null)
            existingUserProfile.AvatarUrl = updateUserProfileRequest.AvatarUrl;

        await _dbContext.SaveChangesAsync();

        return new UserProfileResponse
        {
            Id = existingUserProfile.Id,
            DisplayName = existingUserProfile.DisplayName,
            Email = existingUserProfile.Email,
            FirstName = existingUserProfile.FirstName,
            LastName = existingUserProfile.LastName,
            AvatarUrl = existingUserProfile.AvatarUrl,
        };
    }
}
