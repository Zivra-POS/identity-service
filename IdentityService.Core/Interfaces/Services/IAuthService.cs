using IdentityService.Core.Entities;
using IdentityService.Shared.DTOs.Request;
using IdentityService.Shared.DTOs.Request.Auth;
using IdentityService.Shared.DTOs.Request.User;
using IdentityService.Shared.DTOs.Response;
using IdentityService.Shared.DTOs.Response.Auth;
using System.Threading.Tasks;
using System;
using IdentityService.Shared.DTOs.Response.staff;
using ZivraFramework.Core.Filtering.Entities;
using ZivraFramework.Core.Models;

namespace IdentityService.Core.Interfaces.Services;

public interface IAuthService
{
    Task<User> GetUserByIdAsync(Guid userId);
    Task<User?> GetUserByHashedIdAsync(string hashedId);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<string> LogoutAsync(Guid userId, string refreshToken);
    Task<string> LogoutAllDevicesAsync(Guid userId);
    Task<ForgotPasswordResponse> RequestPasswordResetAsync(ForgotPasswordRequest request);
    Task<string> ResetPasswordAsync(ResetPasswordRequest request);
    Task<UpdateUserResponse> UpdateUserAsync(UpdateUserRequest req);
    Task<RegisterStaffResponse> RegisterStaffAsync(RegisterStaffRequest req);
    Task<string> SendVerifyEmailAsync(SendVerifyEmailRequest req, bool withTxn = true);
    Task<string> VerifyEmailAsync(string code);
    Task<string> UnlockUserAsync(UnlockUserRequest request);
    Task<PagedResult<StaffResponse>> GetStaffByStoreIdAsync(QueryRequest req, Guid storeId);
}