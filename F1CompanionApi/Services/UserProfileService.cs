using System;
using F1CompanionApi.Data;
using F1CompanionApi.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace F1CompanionApi.Services;

public interface IUserProfileService
{
    Task<UserProfile?> GetUserProfileByAccountIdAsync(string accountId);
    Task<UserProfile> CreateUserProfileAsync(string accountId, string email, string? displayName = null);
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
        return await _dbContext.UserProfiles
            .Include(x => x.Account)
            .FirstOrDefaultAsync(x => x.AccountId == accountId);
    }

    public async Task<UserProfile> CreateUserProfileAsync(string accountId, string email, string? displayName = null)
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
}
