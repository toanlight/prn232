using BusinessLayer.Entities.System;
using DataAccessLayer;
using DataAccessLayer.DAO;
using Repositories.Interfaces;

namespace Repositories.Implementations;

public class SystemSettingRepository : GenericRepository<SystemSetting>, ISystemSettingRepository
{
    private readonly SystemSettingDAO _settingDao;

    public SystemSettingRepository(WmsDbContext context) : base(context)
    {
        _settingDao = new SystemSettingDAO(context);
    }

    public async Task<SystemSetting?> GetByKeyAsync(string key) => await _settingDao.GetByKeyAsync(key);
}
