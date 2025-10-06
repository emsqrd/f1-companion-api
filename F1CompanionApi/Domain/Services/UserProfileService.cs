using F1CompanionApi.Data;
using F1CompanionApi.Data.Entities;
using F1CompanionApi.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace F1CompanionApi.Domain.Services;

public interface IUserProfileService
{
    Task<UserProfile?> GetUserProfileByAccountIdAsync(string accountId);
    Task<UserProfile> CreateUserProfileAsync(
        string accountId,
        string email,
        string? displayName = null
    );
    Task<UserProfile> UpdateUserProfileAsync(UserProfileUpdateModel updateModel);
}

public class UserProfileService : IUserProfileService
{
    private readonly ApplicationDbContext _dbContext;

    public UserProfileService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<UserProfile?> GetUserProfileByAccountIdAsync(string accountId)
    {
        return await _dbContext
            .UserProfiles.Include(x => x.Account)
            .FirstOrDefaultAsync(x => x.AccountId == accountId);
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
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
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

    public async Task<UserProfile> UpdateUserProfileAsync(UserProfileUpdateModel updateModel)
    {
        var existingUserProfile = await _dbContext.UserProfiles.FirstOrDefaultAsync(x =>
            x.Id == updateModel.Id
        );

        if (existingUserProfile is null)
            throw new Exception("User doesn't exist");

        if (updateModel.DisplayName is not null)
            existingUserProfile.DisplayName = updateModel.DisplayName;

        if (updateModel.Email is not null)
            existingUserProfile.Email = updateModel.Email;

        if (updateModel.FirstName is not null)
            existingUserProfile.FirstName = updateModel.FirstName;

        if (updateModel.LastName is not null)
            existingUserProfile.LastName = updateModel.LastName;

        if (updateModel.AvatarUrl is not null)
            existingUserProfile.AvatarUrl = updateModel.AvatarUrl;

        existingUserProfile.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return existingUserProfile;
    }
}
