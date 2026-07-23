using BusinessLayer.Entities.Identity;

namespace Service.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(string username, string password);
    Task<LoginResponseDto> RefreshAsync(string refreshToken);
    Task LogoutAsync(int userId, string refreshToken);
    Task ChangePasswordAsync(int userId, string oldPassword, string newPassword);
    Task ForgotPasswordAsync(string email);
    Task ResetPasswordAsync(string token, string newPassword);
}

public record LoginResponseDto(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    UserInfoDto User
);

public record UserInfoDto(
    int UserId,
    string FullName,
    string Email,
    List<string> Roles,
    string PreferredLang
);
