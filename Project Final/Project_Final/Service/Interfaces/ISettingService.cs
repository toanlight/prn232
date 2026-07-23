using BusinessLayer.Entities.System;

namespace Service.Interfaces;

public interface ISettingService
{
    Task<List<SystemSetting>> GetAllAsync();
    Task<SystemSetting?> GetByKeyAsync(string key);
    Task UpdateSettingsAsync(Dictionary<string, string> keyValues);
    Task TestEmailAsync();
}
