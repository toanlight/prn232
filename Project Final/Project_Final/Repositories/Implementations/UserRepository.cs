using BusinessLayer.Entities.Identity;
using DataAccessLayer;
using DataAccessLayer.DAO;
using Repositories.Interfaces;

namespace Repositories.Implementations;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    private readonly UserDAO _userDao;

    public UserRepository(WmsDbContext context) : base(context)
    {
        _userDao = new UserDAO(context);
    }

    public async Task<User?> GetByEmailAsync(string email) => await _userDao.GetByEmailAsync(email);
    public async Task<User?> GetWithRolesByIdAsync(int id) => await _userDao.GetWithRolesByIdAsync(id);
    public async Task<(List<User> Items, int TotalCount)> SearchUsersAsync(string? keyword, bool? isActive, int pageIndex = 1, int pageSize = 10) 
        => await _userDao.SearchUsersAsync(keyword, isActive, pageIndex, pageSize);
}
