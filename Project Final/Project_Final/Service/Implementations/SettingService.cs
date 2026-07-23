using BusinessLayer.Entities.System;
using Repositories.Interfaces;
using Service.Interfaces;

namespace Service.Implementations;

public class SettingService : ISettingService
{
    private readonly ISystemSettingRepository _settingRepo;

    public SettingService(ISystemSettingRepository settingRepo)
    {
        _settingRepo = settingRepo;
    }

    public async Task<List<SystemSetting>> GetAllAsync()
    {
        return (await _settingRepo.GetAllAsync()).ToList();
    }

    public async Task<SystemSetting?> GetByKeyAsync(string key)
    {
        return await _settingRepo.FindAsync(s => s.SettingKey.ToLower() == key.ToLower());
    }

    public async Task UpdateSettingsAsync(Dictionary<string, string> keyValues)
    {
        foreach (var (key, value) in keyValues)
        {
            var setting = await GetByKeyAsync(key);
            if (setting == null)
            {
                setting = new SystemSetting
                {
                    SettingKey = key,
                    SettingValue = value,
                    DataType = "string",
                    UpdatedAt = DateTime.UtcNow
                };
                await _settingRepo.AddAsync(setting);
            }
            else
            {
                setting.SettingValue = value;
                setting.UpdatedAt = DateTime.UtcNow;
                _settingRepo.Update(setting);
            }
        }

        await _settingRepo.SaveChangesAsync();
    }

    public async Task TestEmailAsync()
    {
        // TODO: Test SMTP connection in Phase 4
        await Task.CompletedTask;
    }
}
