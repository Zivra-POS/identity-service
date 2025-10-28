using IdentityService.Shared.DTOs.Request;
using IdentityService.Shared.DTOs.Request.Auth;
using IdentityService.Shared.DTOs.Request.User;
using IdentityService.Shared.DTOs.Response;
using IdentityService.Shared.DTOs.Response.Auth;
using IdentityService.Shared.Response;

namespace IdentityService.Core.Interfaces.Services;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
    Task<Result<string>> LogoutAsync(Guid userId);
    Task<Result<ForgotPasswordResponse>> RequestPasswordResetAsync(ForgotPasswordRequest request);
    Task<Result<string>> ResetPasswordAsync(ResetPasswordRequest request);
    Task<Result<UpdateUserResponse>> UpdateUserAsync(UpdateUserRequest req);
    Task<Result<RegisterStaffResponse>> RegisterStaffAsync(RegisterStaffRequest req);
    Task<string> SendVerifyEmailAsync(SendVerifyEmailRequest req, bool withTxn = true);
    Task<Result<string>> VerifyEmailAsync(string token);
}