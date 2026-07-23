using BusinessLayer.Entities.Identity;

namespace Service.Interfaces;

public interface IUserService
{
    Task<(List<User> Items, int TotalCount)> GetUsersAsync(string? keyword, bool? isActive, int pageIndex = 1, int pageSize = 10);
    Task<User> GetUserByIdAsync(int id);
    Task<User> CreateUserAsync(CreateUserDto dto);
    Task<User> UpdateUserAsync(int id, UpdateUserDto dto);
    Task ToggleActivateAsync(int id);
    Task AssignRolesAsync(int userId, List<int> roleIds);
    Task<User> GetMeAsync(int userId);
    Task UpdateProfileAsync(int userId, UpdateProfileDto dto);
    Task ChangeLanguageAsync(int userId, string lang);
}

public record CreateUserDto(
    string Username,
    string Email,
    string FullName,
    string? FullNameEn,
    string? Phone,
    string Password,
    string PreferredLang,
    List<int> RoleIds
);

public record UpdateUserDto(
    string FullName,
    string? FullNameEn,
    string? Phone,
    string PreferredLang
);

public record UpdateProfileDto(
    string FullName,
    string? FullNameEn,
    string? Phone
);
