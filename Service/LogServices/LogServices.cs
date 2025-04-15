using Core;
using DataLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Service.LogServices;

public interface ILogServices
{
    Task<int> SaveLogAsync(SystemLog systemLog);
    Task<EntityEntry> GetEntryAsync(int logId);
    Task<int> UpdateLogAsync(int logId, SystemLog systemLog);
    Task<int> CountUserLogsAsync(int userId);
    Task<bool> CheckLogExistsAsync(int logId);
    Task<int> DeleteLogAsync(int logId);
    Task<SystemLog?> GetLogByIdWithFindCmdAsync(int logId);
    Task<SystemLog?> GetLogByIdWithSingleCmdAsync(int logId);
    Task<SystemLog?> GetLogByIdWithFirstCmdAsync(int logId);
    Task<bool> ExecuteDeleteSqlRawAsync(int logId);
    Task<bool> ExecuteDeleteSqlInterpolatedAsync(int logId);
    Task<List<SystemLog>> GetLogsAsync();
    Task<List<dynamic>> GetAnonymousLogDataList();
    Task<int> ReadMaxLogIdAsync();
    Task<int> ReadMinLogIdAsync();
    Task<double> ReadAverageLogIdAsync();
    Task<List<dynamic>> ReadGroupedLogsAsync();
    Task<List<dynamic>> ReadJoinedLogsAsync();
    Task<List<dynamic>> ReadLogDescriptionAsync();
}

public class LogServices : ILogServices
{
    private readonly MasterDbContext _masterContext;

    public LogServices(MasterDbContext masterContext)
    {
        _masterContext = masterContext;
    }

    /// <summary>
    /// Check Existed command of efcore With covering conditional checking and Any
    /// </summary>
    public async Task<bool> CheckLogExistsAsync(int logId)
    {
        return await _masterContext.SystemLog.AnyAsync(sl => sl.LogId == logId);
    }

    /// <summary>
    /// Get number count of records with covering conditional search
    /// </summary>
    public async Task<int> CountUserLogsAsync(int userId)
    {
        return await _masterContext.SystemLog.CountAsync(sl => sl.AppUserId == userId);
    }

    /// <summary>
    /// EF Core delete command
    /// </summary>
    public async Task<int> DeleteLogAsync(int logId)
    {
        var dbLog = await GetLogByIdWithFindCmdAsync(logId);
        if (dbLog != null)
        {
            _masterContext.SystemLog.Remove(dbLog);
            await _masterContext.SaveChangesAsync();
            return logId;
        }
        else
            throw new KeyNotFoundException("Cannot found log data");
    }

    /// <summary>
    /// Will run direct sql command to delete a log with parametrizing for better security, will get related sql exception's if failed
    /// </summary>
    public async Task<bool> ExecuteDeleteSqlInterpolatedAsync(int logId)
    {
        var result = await _masterContext.Database.ExecuteSqlInterpolatedAsync($"UPDATE SystemLog SET Deleted = 1 WHERE LogId = {logId}");
        return result == 1 ? true : false;
    }

    /// <summary>
    /// Will run direct sql command to delete a log , will get related sql exception's if failed
    /// </summary>
    public async Task<bool> ExecuteDeleteSqlRawAsync(int logId)
    {
        var result = await _masterContext.Database.ExecuteSqlRawAsync($"UPDATE SystemLog SET Deleted = 1 WHERE LogId = {logId}");
        return result == 1 ? true : false;
    }

    /// <summary>
    /// Creating dynamic anonymous object
    /// </summary>
    public async Task<List<dynamic>> GetAnonymousLogDataList()
    {
        var anonymousList = await _masterContext.SystemLog
        .Select(p => new { p.LogId, p.Description })
        .ToListAsync<dynamic>();
        return anonymousList;
    }

    /// <summary>
    /// Will get entity entry with all data's
    /// </summary>
    public async Task<EntityEntry> GetEntryAsync(int logId)
    {
        var dbLog = await GetLogByIdWithFindCmdAsync(logId);
        var logEntry = _masterContext.SystemLog.Entry(dbLog);
        return logEntry;
    }

    /// <summary>
    /// Fetching data by a key using find async method
    /// </summary>
    public async Task<SystemLog?> GetLogByIdWithFindCmdAsync(int logId)
    {
        return await _masterContext.SystemLog.FindAsync(logId);
    }

    /// <summary>
    /// Fetching data by First method and condition, if multiple found, first one will return, otherwise if nothing found it returns null
    /// </summary>
    public async Task<SystemLog?> GetLogByIdWithFirstCmdAsync(int logId)
    {
        return await _masterContext.SystemLog.FirstOrDefaultAsync(sl => sl.LogId == logId);
    }

    /// <summary>
    /// Fetching data by First method and condition, if multiple found, throws exception, if just one record found, returns that, otherwise it returns null
    /// </summary>
    public async Task<SystemLog?> GetLogByIdWithSingleCmdAsync(int logId)
    {
        return await _masterContext.SystemLog.SingleOrDefaultAsync(sl => sl.LogId == logId);
    }

    /// <summary>
    /// Fetching data list
    /// </summary>
    public async Task<List<SystemLog>> GetLogsAsync()
    {
        return await _masterContext.SystemLog.ToListAsync();
    }

    /// <summary>
    /// Implemented on log id because it was the only numeric field so , don't get so strict ! / Will get average of id values
    /// </summary>
    public async Task<double> ReadAverageLogIdAsync()
    {
        return await _masterContext.SystemLog.AverageAsync(sl => sl.LogId);
    }

    /// <summary>
    /// Will group data based on defined property and get a list of it 
    /// </summary>
    public async Task<List<dynamic>> ReadGroupedLogsAsync()
    {
        var groupedLogs = await _masterContext.SystemLog
            .GroupBy(p => p.AppUserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToListAsync<dynamic>();
        return groupedLogs;
    }

    /// <summary>
    /// Will join two datasets based on key's that we provide
    /// </summary>
    public async Task<List<dynamic>> ReadJoinedLogsAsync()
    {
        var userLogs = await _masterContext.SystemLog
            .Join(_masterContext.AppUser,
            sl => sl.AppUserId,
            au => au.AppUserId,
            (sl, au) => new { sl.Description, au.Username, au.EmailAddress })
            .ToListAsync<dynamic>();
        return userLogs;
    }

    /// <summary>
    /// Fetching anonymous data , just specific field
    /// </summary>
    public async Task<List<dynamic>> ReadLogDescriptionAsync()
    {
        return await _masterContext.SystemLog.Select(sl => sl.Description).ToListAsync<dynamic>();
    }

    /// <summary>
    /// Will get maximum log id from database
    /// </summary>
    public async Task<int> ReadMaxLogIdAsync()
    {
        return await _masterContext.SystemLog.MaxAsync(sl => sl.LogId);
    }

    /// <summary>
    /// Will get minumum log id from database
    /// </summary>
    public async Task<int> ReadMinLogIdAsync()
    {
        return await _masterContext.SystemLog.MinAsync(sl => sl.LogId);
    }

    /// <summary>
    /// Insert command of ef core
    /// </summary>
    public async Task<int> SaveLogAsync(SystemLog systemLog)
    {
        await _masterContext.AddAsync(systemLog);
        await _masterContext.SaveChangesAsync();

        return systemLog.LogId;
    }

    /// <summary>
    /// Update command of ef core
    /// </summary>
    public async Task<int> UpdateLogAsync(int logId, SystemLog systemLog)
    {
        var dbLog = await GetLogByIdWithFindCmdAsync(logId);
        if (dbLog == null)
            throw new KeyNotFoundException("Log data not found");

        dbLog.Description = systemLog.Description;
        dbLog.LogSerial = systemLog.LogSerial;
        dbLog.LogDateTime = systemLog.LogDateTime;

        _masterContext.SystemLog.Update(dbLog);
        await _masterContext.SaveChangesAsync();

        return logId;
    }
}