using Microsoft.AspNetCore.Mvc;
using IdentityService.Core.Interfaces.Services;
using IdentityService.Core.Mappers.Auth;
using IdentityService.Shared.DTOs.Request.Auth;
using IdentityService.Shared.DTOs.Request.User;
using IdentityService.Shared.Response;
using Microsoft.AspNetCore.Authorization;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _config;

    public AuthController(IAuthService authService, IRefreshTokenService refreshTokenService, ITokenService tokenService, IConfiguration config)
    {
        _authService = authService;
        _refreshTokenService = refreshTokenService;
        _tokenService = tokenService;
        _config = config;
    }

    #region Register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var r = await _authService.RegisterAsync(req);
        if (!r.IsSuccess)
            return StatusCode((int)r.StatusCode, r.Message);

        return StatusCode(StatusCodes.Status201Created, r);
    }
    #endregion
    
    #region RegisterStaff
    [HttpPost("register-staff")]
    public async Task<IActionResult> RegisterStaff([FromForm] RegisterStaffRequest req)
    {
        var r = await _authService.RegisterStaffAsync(req);
        return StatusCode((int)r.StatusCode, r);
    }
    #endregion

    #region UpdateUserAsync
    [HttpPut()]
    public async Task<IActionResult> UpdateUserAsync([FromForm] UpdateUserRequest req)
    {
        var r = await _authService.UpdateUserAsync(req);
        return StatusCode((int)r.StatusCode, r);
    }
    #endregion
    
    #region Login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var r = await _authService.LoginAsync(req);
        if (!r.IsSuccess)
            return StatusCode((int)r.StatusCode, r);

        var auth = r.Data!;

        var jwtHours = int.TryParse(_config["JwtSettings:ExpireHours"], out var h) ? h : 3;
        var refreshDays = int.TryParse(_config["JwtSettings:RefreshTokenDays"], out var d) ? d : 30;

        var accessCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddHours(jwtHours)
        };

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(refreshDays)
        };

        Response.Cookies.Append("access_token", auth.Token, accessCookieOptions);
        Response.Cookies.Append("refresh_token", auth.RefreshToken, refreshCookieOptions);

        return StatusCode((int)r.StatusCode, r);
    }
    #endregion
    
    #region RefreshToken
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        if (!Request.Cookies.TryGetValue("refresh_token", out var existingRaw) || string.IsNullOrEmpty(existingRaw))
            return Unauthorized(new { message = "Refresh token tidak ditemukan." });

        var rotated = await _refreshTokenService.RotateRefreshTokenAsync(existingRaw);
        if (rotated == null)
            return Unauthorized(new { message = "Refresh token tidak valid atau sudah kedaluwarsa." });

        var (user, newRawToken) = rotated.Value;
        
        var roleNames = user.UserRoles.Select(ur => ur.Role?.Name).Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => r!).ToArray();

        var newJwt = _tokenService.GenerateJwtToken(user, roleNames);
        var resp = AuthMapper.ToAuthResponse(user, roleNames, newJwt, newRawToken);

        var jwtHours = int.TryParse(_config["JwtSettings:ExpireHours"], out var h) ? h : 3;
        var refreshDays = int.TryParse(_config["JwtSettings:RefreshTokenDays"], out var d) ? d : 30;

        var accessCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddHours(jwtHours)
        };

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(refreshDays)
        };

        Response.Cookies.Append("access_token", newJwt, accessCookieOptions);
        Response.Cookies.Append("refresh_token", newRawToken, refreshCookieOptions);

        return Ok(resp);
    }
    #endregion

    #region Logout
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrWhiteSpace(sub) || !Guid.TryParse(sub, out var userId))
            return Unauthorized(new { message = "User tidak terautentikasi." });

        var r = await _authService.LogoutAsync(userId);
        if (!r.IsSuccess)
            return StatusCode((int)r.StatusCode, r);
        
        Response.Cookies.Delete("access_token");
        Response.Cookies.Delete("refresh_token");

        return StatusCode((int)r.StatusCode, r);
    }
    #endregion

    #region ForgotPassword
    [AllowAnonymous]
    [HttpPost("forgot")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
    {
        var r = await _authService.RequestPasswordResetAsync(req);
        return StatusCode((int)r.StatusCode, r);
    }
    #endregion

    #region ResetPassword
    [AllowAnonymous]
    [HttpPost("reset")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
    {
        var r = await _authService.ResetPasswordAsync(req);
        return StatusCode((int)r.StatusCode, r);
    }
    #endregion
    
    #region SendVerifyEmail
    [HttpPost("send-verify-email")]
    public async Task<IActionResult> SendVerifyEmail([FromBody] SendVerifyEmailRequest req)
    {
        var r = await _authService.SendVerifyEmailAsync(req);
        
        return Ok(Result<string>.Success(r));
    }
    #endregion
    
    #region VerifyEmail
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(string token)
    {
        var r = await _authService.VerifyEmailAsync(token);
        return StatusCode((int)r.StatusCode, r);
    }
    #endregion
}