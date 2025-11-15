using Microsoft.AspNetCore.Mvc;
using IdentityService.Core.Interfaces.Services;
using IdentityService.Core.Mappers;
using IdentityService.Shared.DTOs.Request.Auth;
using IdentityService.Shared.DTOs.Request.User;
using IdentityService.Shared.DTOs.Response.Auth;
using IdentityService.Shared.Response;
using Microsoft.AspNetCore.Authorization;
using ZivraFramework.Core.API.Exception;
using ZivraFramework.Core.API.Helpers;
using ZivraFramework.Core.Interfaces;
using ZivraFramework.Core.Utils;
using Microsoft.AspNetCore.Http;
using ZivraFramework.Core.Filtering.Entities;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ITokenService _tokenService;
    private readonly ICurrentUserService _currentUser;
    private readonly IConfiguration _config;
    private readonly IUnitOfWork _unitOfWork;

    public AuthController(
        IAuthService authService,
        IRefreshTokenService refreshTokenService,
        ITokenService tokenService,
        IConfiguration config,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _authService = authService;
        _refreshTokenService = refreshTokenService;
        _tokenService = tokenService;
        _config = config;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    #region Register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var r = await _authService.RegisterAsync(req);
        return StatusCode(StatusCodes.Status201Created, r);
    }
    #endregion
    
    #region RegisterStaff
    [Authorize]
    [HttpPost("register-staff")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> RegisterStaff([FromForm] RegisterStaffRequest req)
    {
        req.OwnerId ??= _currentUser.UserId;
        var res = await _authService.RegisterStaffAsync(req);
        return StatusCode(StatusCodes.Status201Created, res);
    }
    #endregion

    #region UpdateUserAsync
    [Authorize]
    [HttpPut()]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateUserAsync([FromForm] UpdateUserRequest req)
    {
        var res = await _authService.UpdateUserAsync(req);
        return Ok(res);
    }
    #endregion
    
    #region Login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var auth = await _authService.LoginAsync(req);

        var jwtHours = int.TryParse(_config["JwtSettings:ExpireHours"], out var h) ? h : 3;
        var refreshDays = int.TryParse(_config["JwtSettings:RefreshTokenDays"], out var d) ? d : 30;

        var accessCookieOptions = new CookieOptions
        {
            HttpOnly = false,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddHours(jwtHours),
            Path = "/",
            Domain = null
        };

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = false,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(refreshDays),
            Path = "/",
            Domain = null
        };

        Response.Cookies.Append("access_token", auth.Token, accessCookieOptions);
        Response.Cookies.Append("refresh_token", auth.RefreshToken, refreshCookieOptions);

        return Ok(auth);
    }
    #endregion
    
    #region RefreshToken
    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var existingRaw = RefreshTokenHelper.GetRefreshTokenFromRequest(Request);
            if (string.IsNullOrEmpty(existingRaw))
                throw new UnauthorizedException("Refresh token tidak ditemukan.");
            
            var existingToken = await _refreshTokenService.GetByRefreshTokenHashAsync(existingRaw!);
            if (existingToken is null)
                throw new UnauthorizedException("Refresh token tidak valid atau sudah kadaluarsa.");

            var user = await _authService.GetUserByIdAsync(existingToken.UserId);
            var userRoles = user.UserRoles
                .Select(ur => ur.Role?.Name)
                .Where(rn => !string.IsNullOrWhiteSpace(rn))
                .Select(rn => rn!).ToList();

            var newJwt = await _tokenService.GenerateJwtToken(user, userRoles);
            var newRefreshToken = await _refreshTokenService.RotateRefreshTokenAsync(existingRaw, newJwt.Id);
            
            if (newRefreshToken is null)
                throw new UnauthorizedException("Refresh token tidak valid atau sudah kadaluarsa.");
            
            var resp = AuthMapper.ToAuthResponse(user, userRoles, newJwt.Token, newRefreshToken);

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

            Response.Cookies.Append("access_token", newJwt.Token, accessCookieOptions);
            Response.Cookies.Append("refresh_token", newRefreshToken, refreshCookieOptions);
            
            await _unitOfWork.CommitTransactionAsync();
            
            Logger.Info("Berhasil merotasi refresh token untuk user ID: " + user.Id);
            return Ok(resp);
        }
        catch (Exception e)
        {
            Logger.Error("Gagal merotasi refresh token: " + e.Message);
            throw;
        }
    }
    #endregion

    #region Logout
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = _currentUser.UserId;
        
        var refreshToken = RefreshTokenHelper.GetRefreshTokenFromRequest(Request);
        if (string.IsNullOrWhiteSpace(refreshToken))
            return BadRequest(new { message = "Refresh token tidak ditemukan." });

        var msg = await _authService.LogoutAsync(userId, refreshToken);
        
        Response.Cookies.Delete("access_token");
        Response.Cookies.Delete("refresh_token");

        return Ok(new { message = msg });
    }
    #endregion

    #region LogoutAllDevices
    [HttpPost("logout-all-devices")]
    [Authorize]
    public async Task<IActionResult> LogoutAllDevices()
    {
        var userId = _currentUser.UserId;

        var msg = await _authService.LogoutAllDevicesAsync(userId);
        
        Response.Cookies.Delete("access_token");
        Response.Cookies.Delete("refresh_token");

        return Ok(new { message = msg });
    }
    #endregion

    #region ForgotPassword
    [AllowAnonymous]
    [HttpPost("forgot")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
    {
        var resp = await _authService.RequestPasswordResetAsync(req);
        return Ok(resp);
    }
    #endregion

    #region ResetPassword
    [AllowAnonymous]
    [HttpPost("reset")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
    {
        var msg = await _authService.ResetPasswordAsync(req);
        return Ok(new { message = msg });
    }
    #endregion
    
    #region SendVerifyEmail
    [HttpPost("send-verify-email")]
    public async Task<IActionResult> SendVerifyEmail([FromBody] SendVerifyEmailRequest req)
    {
        var code = await _authService.SendVerifyEmailAsync(req);
        return Ok(new { code = code });
    }
    #endregion
    
    #region VerifyEmail
    [AllowAnonymous]
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string code)
    {
        var msg = await _authService.VerifyEmailAsync(code);
        return Ok(new { message = msg });
    }
    #endregion
    
    #region UnlockUser
    [HttpPost("unlock-user")]
    [Authorize]
    public async Task<IActionResult> UnlockUser([FromBody] UnlockUserRequest req)
    {
        var msg = await _authService.UnlockUserAsync(req);
        return Ok(new { message = msg });
    }
    #endregion

    #region Me
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        return Ok(new
        {
            Id = _currentUser.UserId,
            _currentUser.Username,
            _currentUser.FullName,
            _currentUser.Email,
            _currentUser.Roles,
            _currentUser.StoreId
        });
    }
    #endregion

    #region GetUserByHashedId
    [HttpGet("user/hashed/{hashedId}")]
    public async Task<IActionResult> GetUserByHashedId(string hashedId)
    {
        var user = await _authService.GetUserByHashedIdAsync(hashedId);
        if (user == null)
            return NotFound(new { message = "User tidak ditemukan." });

        var roles = user.UserRoles?.Select(ur => ur.Role?.Name).Where(rn => !string.IsNullOrWhiteSpace(rn)).Select(rn => rn!).ToArray() ?? Array.Empty<string>();

        return Ok(new
        {
            Id = user.Id,
            user.Username,
            user.FullName,
            user.Email,
            Roles = roles,
            user.StoreId
        });
    }
    #endregion
    
    #region GetStaffByStoreIdAsync
    [HttpGet("staff-by-store")]
    [Authorize]
    public async Task<IActionResult> GetStaffByStoreIdAsync([FromQuery] QueryRequest req)
    {
        var result = await _authService.GetStaffByStoreIdAsync(req);
        return Ok(result);
    }
    #endregion

}
