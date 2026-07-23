using Microsoft.EntityFrameworkCore;
using BusinessLayer.Entities.System;

namespace DataAccessLayer.DAO;

public class SystemSettingDAO : GenericDAO<SystemSetting>
{
    public SystemSettingDAO(WmsDbContext context) : base(context)
    {
    }

    public async Task<SystemSetting?> GetByKeyAsync(string settingKey)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.SettingKey.ToLower() == settingKey.ToLower());
    }

    public async Task<string?> GetValueByKeyAsync(string settingKey)
    {
        var setting = await GetByKeyAsync(settingKey);
        return setting?.SettingValue;
    }
}
