using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Web.Services;

namespace WMS.Web.Controllers;

public class AuthController : Controller
{
    private readonly IWmsApiClient _apiClient;

    public AuthController(IWmsApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
    {
        try
        {
            var response = await _apiClient.PostAsync<ApiResponseDto<LoginResponseDto>>("api/auth/login", new { username, password });

            if (response != null && response.Success && response.Data != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, response.Data.User.UserId.ToString()),
                    new Claim(ClaimTypes.Name, response.Data.User.FullName),
                    new Claim(ClaimTypes.Email, response.Data.User.Email),
                    new Claim("JWToken", response.Data.AccessToken)
                };

                foreach (var role in response.Data.User.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Dashboard");
            }

            ViewBag.Error = response?.Message ?? "Tên đăng nhập hoặc mật khẩu không chính xác.";
        }
        catch (Exception ex)
        {
            ViewBag.Error = "Không thể kết nối đến máy chủ API: " + ex.Message;
        }

        return View();
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    [HttpGet]
    [Authorize]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View();
    }
}

public class ApiResponseDto<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public int StatusCode { get; set; }
}

public class LoginResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public UserInfoDto User { get; set; } = default!;
}

public class UserInfoDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public string PreferredLang { get; set; } = string.Empty;
}
