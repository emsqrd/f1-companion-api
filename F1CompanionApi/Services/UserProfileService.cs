using System;
using F1CompanionApi.Data;
using F1CompanionApi.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace F1CompanionApi.Services;

public interface IUserProfileService
{
    Task<UserProfile?> GetUserProfileByAccountIdAsync(string accountId);
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
}
