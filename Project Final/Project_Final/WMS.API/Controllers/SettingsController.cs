using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using WMS.API.Infrastructure;

namespace WMS.API.Controllers;

[ApiController]
[Route("api/settings")]
[Authorize(Roles = "ADMIN")]
public class SettingsController : ControllerBase
{
    private readonly ISettingService _settingService;

    public SettingsController(ISettingService settingService)
    {
        _settingService = settingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var settings = await _settingService.GetAllAsync();
        return Ok(ApiResponse<object>.Ok(settings));
    }

    [HttpGet("{key}")]
    public async Task<IActionResult> GetByKey(string key)
    {
        var setting = await _settingService.GetByKeyAsync(key);
        if (setting == null)
            return NotFound(ApiResponse.Error($"Setting with key '{key}' not found", 404));

        return Ok(ApiResponse<object>.Ok(setting));
    }

    [HttpPut]
    public async Task<IActionResult> UpdateSettings([FromBody] Dictionary<string, string> keyValues)
    {
        await _settingService.UpdateSettingsAsync(keyValues);
        return Ok(ApiResponse.OkMessage("Settings updated successfully"));
    }

    [HttpPost("test-email")]
    public async Task<IActionResult> TestEmail()
    {
        await _settingService.TestEmailAsync();
        return Ok(ApiResponse.OkMessage("Test email initiated"));
    }
}
