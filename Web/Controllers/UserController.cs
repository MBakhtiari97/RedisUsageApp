using Core;
using Microsoft.AspNetCore.Mvc;
using Service.UserServices;

namespace Web.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IUserServices _userServices;

    public UserController(ILogger<UserController> logger, IUserServices userServices)
    {
        _userServices = userServices;
        _logger = logger;
    }

    [HttpGet]
    [Route("[action]")]
    public async Task<IActionResult> GetUser(int userId)
    {
        try
        {
            var result = await _userServices.GetUserAsync(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    [HttpGet]
    [Route("[action]")]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            var result = await _userServices.GetAllUsersAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> SaveUser(AppUser user)
    {
        try
        {
            var result = await _userServices.SaveUserAsync(user);
            return Ok(result);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    [HttpPut]
    [Route("[action]")]
    public async Task<IActionResult> UpdateUser(int userId, AppUser user)
    {
        try
        {
            var result = await _userServices.UpdateUserAsync(userId, user);
            return Ok(result);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    [HttpDelete]
    [Route("[action]")]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        try
        {
            var result = await _userServices.DeleteUserAsync(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    [HttpGet]
    [Route("[action]")]
    public async Task<IActionResult> GetFlattenUserLogColleciton()
    {
        try
        {
            var result = await _userServices.GetFlattenCollectionUserAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    [HttpGet]
    [Route("[action]")]
    public async Task<IActionResult> GetCrossUserLogColleciton()
    {
        try
        {
            var result = await _userServices.GetCrossUserLogsAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}