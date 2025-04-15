using Core;
using DataLayer;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;

namespace Service.UserServices;

public interface IUserServices
{
    Task<int> SaveUserAsync(AppUser user);
    Task<int> UpdateUserAsync(int userId, AppUser user);
    Task<AppUser?> GetUserAsync(int userId);
    Task<int> DeleteUserAsync(int userId);
    Task<IEnumerable<AppUser>?> GetAllUsersAsync();
    Task<AppUser?> GetUserWithLogsAsync(int userId);
    Task<IEnumerable<AppUser>?> GetUsersWithLogsAsync();
    Task<List<dynamic>> GetFlattenCollectionUserAsync();
    Task<List<dynamic>> GetCrossUserLogsAsync();
}

public class UserServices : IUserServices
{
    private readonly MasterDbContext _masterContext;

    public UserServices(MasterDbContext masterContext)
    {
        _masterContext = masterContext;
    }

    /// <summary>
    /// Delete command of efcore and it's usage sample
    /// </summary>
    public async Task<int> DeleteUserAsync(int userId)
    {
        var user = await GetUserAsync(userId);
        if (user == null)
            throw new Exception("User not found");

        user.Deleted = true;
        _masterContext.AppUser.Update(user);

        await _masterContext.SaveChangesAsync();

        return user.AppUserId;
    }

    /// <summary>
    /// Fetching all data (can modify with WHERE)
    /// </summary>
    public async Task<IEnumerable<AppUser>?> GetAllUsersAsync()
    {
        return await _masterContext.AppUser
        .ToListAsync();
    }

    /// <summary>
    /// Using SelectMany to Generating Cross-Users(Cartesian User)(Fetching data)
    /// </summary>
    public async Task<List<dynamic>> GetCrossUserLogsAsync()
    {
        var users = await _masterContext.AppUser.ToListAsync();
        var logs = await _masterContext.SystemLog.ToListAsync();

        // Generating all combinations of users and logs
        var userLogPair = users
            .SelectMany(users => logs,
            (user, log) => new { Username = user.Username, Description = log.Description });

        var result = new List<dynamic>();
        foreach (var pair in userLogPair)
        {
            string text = $"{pair.Username} submitted operation with description of {pair.Description}";
            result.Add(text);
        }
        return result;
    }

    /// <summary>
    /// Using SelectMany to flatten the SystemLogs collection (Fetching data)
    /// </summary>
    public async Task<List<dynamic>> GetFlattenCollectionUserAsync()
    {
        var systemLogs = await _masterContext.AppUser.SelectMany(c => c.SystemLogs).ToListAsync();
        var result = new List<dynamic>();
        foreach (var systemLog in systemLogs)
            result.Add(systemLog);
        return result;
    }

    /// <summary>
    /// Fetching data with first or default (more descripted caption in log services)
    /// </summary>
    public async Task<AppUser?> GetUserAsync(int userId)
    {
        return await _masterContext.AppUser
            .FirstOrDefaultAsync(x => x.AppUserId == userId);
    }

    /// <summary>
    /// Fetching data with include and list command , to fetching also relational data
    /// </summary>
    public async Task<IEnumerable<AppUser>?> GetUsersWithLogsAsync()
    {
        return await _masterContext.AppUser.Include(au => au.SystemLogs).ToListAsync();
    }

    /// <summary>
    /// Fetching data with first or default (more descripted caption in log services)
    /// </summary>
    public async Task<AppUser?> GetUserWithLogsAsync(int userId)
    {
        return await _masterContext.AppUser.Include(au => au.SystemLogs).FirstOrDefaultAsync(q => q.AppUserId == userId);
    }

    /// <summary>
    /// Inserting new record command
    /// </summary>
    public async Task<int> SaveUserAsync(AppUser user)
    {
        await _masterContext.AddAsync(user);
        await _masterContext.SaveChangesAsync();
        return user.AppUserId;
    }

    /// <summary>
    /// Updating command of ef core
    /// </summary>
    public async Task<int> UpdateUserAsync(int userId, AppUser user)
    {
        var dbUser = await GetUserAsync(userId);
        if (user == null)
            throw new Exception("User not found");

        dbUser.Username = user.Username;
        dbUser.EmailAddress = user.EmailAddress;
        dbUser.Password = user.Password;

        _masterContext.AppUser.Update(dbUser);

        await _masterContext.SaveChangesAsync();

        return dbUser.AppUserId;
    }
}