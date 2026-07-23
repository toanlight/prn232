using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BusinessLayer.Entities.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repositories.Interfaces;
using Service.Interfaces;

namespace Service.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IRefreshTokenRepository _tokenRepo;
    private readonly IConfiguration _config;

    public AuthService(
        IUserRepository userRepo,
        IRefreshTokenRepository tokenRepo,
        IConfiguration config)
    {
        _userRepo = userRepo;
        _tokenRepo = tokenRepo;
        _config = config;
    }

    public async Task<LoginResponseDto> LoginAsync(string username, string password)
    {
        var user = await _userRepo.FindAsync(u =>
            u.Username.ToLower() == username.ToLower() ||
            u.Email.ToLower() == username.ToLower())
            ?? throw new UnauthorizedAccessException("Tên đăng nhập hoặc mật khẩu không đúng.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Tài khoản đã bị vô hiệu hóa.");

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Tên đăng nhập hoặc mật khẩu không đúng.");

        var userWithRoles = await _userRepo.GetWithRolesByIdAsync(user.Id)
            ?? throw new InvalidOperationException("Không tìm thấy thông tin người dùng.");

        var roles = userWithRoles.UserRoles?.Select(ur => ur.Role.RoleCode).ToList() ?? new();
        var accessToken = GenerateAccessToken(user, roles);
        var refreshToken = await GenerateAndSaveRefreshTokenAsync(user.Id);

        return new LoginResponseDto(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            ExpiresIn: 3600,
            User: new UserInfoDto(user.Id, user.FullName, user.Email, roles, user.PreferredLang ?? "vi")
        );
    }

    public async Task<LoginResponseDto> RefreshAsync(string refreshToken)
    {
        var token = await _tokenRepo.GetByTokenAsync(refreshToken)
            ?? throw new UnauthorizedAccessException("Refresh token không hợp lệ.");

        if (token.IsRevoked || token.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token đã hết hạn hoặc bị thu hồi.");

        var user = await _userRepo.GetWithRolesByIdAsync(token.UserId)
            ?? throw new InvalidOperationException("Không tìm thấy người dùng.");

        var roles = user.UserRoles?.Select(ur => ur.Role.RoleCode).ToList() ?? new();
        var newAccessToken = GenerateAccessToken(user, roles);
        var newRefreshToken = await GenerateAndSaveRefreshTokenAsync(user.Id);

        // Revoke old token
        token.IsRevoked = true;
        _tokenRepo.Update(token);
        await _tokenRepo.SaveChangesAsync();

        return new LoginResponseDto(
            AccessToken: newAccessToken,
            RefreshToken: newRefreshToken,
            ExpiresIn: 3600,
            User: new UserInfoDto(user.Id, user.FullName, user.Email, roles, user.PreferredLang ?? "vi")
        );
    }

    public async Task LogoutAsync(int userId, string refreshToken)
    {
        await _tokenRepo.RevokeTokenAsync(refreshToken);
        await _tokenRepo.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(int userId, string oldPassword, string newPassword)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

        if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
            throw new InvalidOperationException("Mật khẩu cũ không đúng.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        _userRepo.Update(user);
        await _userRepo.SaveChangesAsync();
    }

    public async Task ForgotPasswordAsync(string email)
    {
        var user = await _userRepo.FindAsync(u => u.Email.ToLower() == email.ToLower());
        if (user == null) return; // Không tiết lộ email có tồn tại hay không

        var resetToken = Guid.NewGuid().ToString("N");
        user.PasswordResetToken = resetToken;
        user.PasswordResetExpiry = DateTime.UtcNow.AddHours(2);
        _userRepo.Update(user);
        await _userRepo.SaveChangesAsync();

        // TODO: Gửi email qua INotificationService khi implement Phase 4
    }

    public async Task ResetPasswordAsync(string token, string newPassword)
    {
        var user = await _userRepo.FindAsync(u =>
            u.PasswordResetToken == token &&
            u.PasswordResetExpiry > DateTime.UtcNow)
            ?? throw new InvalidOperationException("Token reset mật khẩu không hợp lệ hoặc đã hết hạn.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.PasswordResetToken = null;
        user.PasswordResetExpiry = null;
        _userRepo.Update(user);
        await _userRepo.SaveChangesAsync();
    }

    // Private helpers
    private string GenerateAccessToken(User user, List<string> roles)
    {
        var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key chưa được cấu hình.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("fullName", user.FullName),
            new("preferredLang", user.PreferredLang ?? "vi"),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<string> GenerateAndSaveRefreshTokenAsync(int userId)
    {
        var tokenValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = tokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };
        await _tokenRepo.AddAsync(refreshToken);
        await _tokenRepo.SaveChangesAsync();
        return tokenValue;
    }
}
