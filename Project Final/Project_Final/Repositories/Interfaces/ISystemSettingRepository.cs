using BusinessLayer.Entities.System;

namespace Repositories.Interfaces;

public interface ISystemSettingRepository : IGenericRepository<SystemSetting>
{
    Task<SystemSetting?> GetByKeyAsync(string key);
}
