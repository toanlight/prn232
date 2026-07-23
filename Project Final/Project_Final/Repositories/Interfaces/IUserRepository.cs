using BusinessLayer.Entities.Identity;

namespace Repositories.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetWithRolesByIdAsync(int id);
    Task<(List<User> Items, int TotalCount)> SearchUsersAsync(string? keyword, bool? isActive, int pageIndex = 1, int pageSize = 10);
}
