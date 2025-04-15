using Core;
using Microsoft.AspNetCore.Mvc;
using Service.LogServices;

namespace Web.Controllers;

[ApiController]
[Route("[controller]")]
public class LogController : ControllerBase
{
    private readonly ILogger<LogController> _logger;
    private readonly ILogServices _logServices;

    public LogController(ILogger<LogController> logger, ILogServices logServices)
    {
        _logServices = logServices;
        _logger = logger;
    }

    [HttpGet]
    [Route("[action]")]
    public async Task<IActionResult> GetLog(int logId)
    {
        try
        {
            var result = await _logServices.GetLogByIdWithFindCmdAsync(logId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    [HttpGet]
    [Route("[action]")]
    public async Task<IActionResult> GetLogs()
    {
        try
        {
            var result = await _logServices.GetLogsAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> SaveLog(SystemLog log)
    {
        try
        {
            var result = await _logServices.SaveLogAsync(log);
            return Ok(result);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    [HttpPut]
    [Route("[action]")]
    public async Task<IActionResult> UpdateLog(int logId, SystemLog log)
    {
        try
        {
            var result = await _logServices.UpdateLogAsync(logId, log);
            return Ok(result);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    [HttpDelete]
    [Route("[action]")]
    public async Task<IActionResult> DeleteLog(int logId)
    {
        try
        {
            var result = await _logServices.DeleteLogAsync(logId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    [HttpDelete]
    [Route("[action]")]
    public async Task<IActionResult> RawDelete(int logId)
    {
        try
        {
            var result = await _logServices.ExecuteDeleteSqlRawAsync(logId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    [HttpDelete]
    [Route("[action]")]
    public async Task<IActionResult> RawDelete2(int logId)
    {
        try
        {
            var result = await _logServices.ExecuteDeleteSqlInterpolatedAsync(logId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}