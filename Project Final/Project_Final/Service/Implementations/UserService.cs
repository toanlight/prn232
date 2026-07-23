using BusinessLayer.Entities.Identity;
using Repositories.Interfaces;
using Service.Interfaces;

namespace Service.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepo;
    private readonly IRoleRepository _roleRepo;

    public UserService(IUserRepository userRepo, IRoleRepository roleRepo)
    {
        _userRepo = userRepo;
        _roleRepo = roleRepo;
    }

    public async Task<(List<User> Items, int TotalCount)> GetUsersAsync(
        string? keyword, bool? isActive, int pageIndex = 1, int pageSize = 10)
        => await _userRepo.SearchUsersAsync(keyword, isActive, pageIndex, pageSize);

    public async Task<User> GetUserByIdAsync(int id)
        => await _userRepo.GetWithRolesByIdAsync(id)
           ?? throw new KeyNotFoundException($"Không tìm thấy người dùng ID={id}.");

    public async Task<User> CreateUserAsync(CreateUserDto dto)
    {
        if (await _userRepo.ExistsAsync(u => u.Email.ToLower() == dto.Email.ToLower()))
            throw new InvalidOperationException($"Email '{dto.Email}' đã tồn tại.");
        if (await _userRepo.ExistsAsync(u => u.Username.ToLower() == dto.Username.ToLower()))
            throw new InvalidOperationException($"Username '{dto.Username}' đã tồn tại.");

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            FullName = dto.FullName,
            FullNameEn = dto.FullNameEn,
            Phone = dto.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            PreferredLang = dto.PreferredLang,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepo.AddAsync(user);
        await _userRepo.SaveChangesAsync();

        if (dto.RoleIds.Count > 0)
            await AssignRolesAsync(user.Id, dto.RoleIds);

        return user;
    }

    public async Task<User> UpdateUserAsync(int id, UpdateUserDto dto)
    {
        var user = await _userRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy người dùng ID={id}.");

        user.FullName = dto.FullName;
        user.FullNameEn = dto.FullNameEn;
        user.Phone = dto.Phone;
        user.PreferredLang = dto.PreferredLang;
        user.UpdatedAt = DateTime.UtcNow;

        _userRepo.Update(user);
        await _userRepo.SaveChangesAsync();
        return user;
    }

    public async Task ToggleActivateAsync(int id)
    {
        var user = await _userRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy người dùng ID={id}.");

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        _userRepo.Update(user);
        await _userRepo.SaveChangesAsync();
    }

    public async Task AssignRolesAsync(int userId, List<int> roleIds)
    {
        var user = await _userRepo.GetWithRolesByIdAsync(userId)
            ?? throw new KeyNotFoundException($"Không tìm thấy người dùng ID={userId}.");

        // Xóa tất cả role cũ
        user.UserRoles.Clear();

        // Thêm role mới
        foreach (var roleId in roleIds.Distinct())
        {
            if (!await _roleRepo.ExistsAsync(r => r.Id == roleId))
                throw new KeyNotFoundException($"Không tìm thấy Role ID={roleId}.");

            user.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId });
        }

        _userRepo.Update(user);
        await _userRepo.SaveChangesAsync();
    }

    public async Task<User> GetMeAsync(int userId)
        => await _userRepo.GetWithRolesByIdAsync(userId)
           ?? throw new KeyNotFoundException("Không tìm thấy thông tin người dùng.");

    public async Task UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("Không tìm thấy thông tin người dùng.");

        user.FullName = dto.FullName;
        user.FullNameEn = dto.FullNameEn;
        user.Phone = dto.Phone;
        user.UpdatedAt = DateTime.UtcNow;

        _userRepo.Update(user);
        await _userRepo.SaveChangesAsync();
    }

    public async Task ChangeLanguageAsync(int userId, string lang)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("Không tìm thấy thông tin người dùng.");

        user.PreferredLang = lang;
        user.UpdatedAt = DateTime.UtcNow;
        _userRepo.Update(user);
        await _userRepo.SaveChangesAsync();
    }
}
