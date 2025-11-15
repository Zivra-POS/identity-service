using Grpc.Core;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ZivraFramework.Core.Utils;

namespace IdentityService.Infrastructure.Services.Grpc;

public class GrpcAuthService : AuthenticationService.AuthenticationServiceBase
{
    private readonly IdentityDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IUserRepository _userRepository;

    public GrpcAuthService(
        IdentityDbContext context,
        IConfiguration configuration, 
        IUserRepository userRepository)
    {
        _context = context;
        _configuration = configuration;
        _userRepository = userRepository;
    }

    public override async Task<ValidateTokenResponse> ValidateToken(ValidateTokenRequest request, ServerCallContext context)
    {
        try
        {
            var validationResult = await ValidateTokenInternal(request.Token);
            return validationResult;
        }
        catch (Exception ex)
        {
            Logger.Error("Gagal memvalidasi token.", ex);
            return new ValidateTokenResponse
            {
                IsValid = false,
                ErrorMessage = "Internal server error"
            };
        }
    }

    public override async Task<GetUserInfoResponse> GetUserInfo(GetUserInfoRequest request, ServerCallContext context)
    {
        try
        {
            var validationResult = await ValidateTokenInternal(request.Token);
            
            return new GetUserInfoResponse
            {
                IsValid = validationResult.IsValid,
                UserId = validationResult.UserId,
                Username = validationResult.Username,
                FullName = validationResult.FullName,
                Email = validationResult.Email,
                Roles = { validationResult.Roles },
                ErrorMessage = validationResult.ErrorMessage
            };
        }
        catch (Exception ex)
        {
            Logger.Error("Gagal mendapatkan informasi user.", ex);
            return new GetUserInfoResponse
            {
                IsValid = false,
                ErrorMessage = "Internal server error"
            };
        }
    }

    private async Task<ValidateTokenResponse> ValidateTokenInternal(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return new ValidateTokenResponse
            {
                IsValid = false,
                ErrorMessage = "Token is required"
            };
        }

        try
        {
            var tokenRecord = await _context.AccessTokens.FirstOrDefaultAsync(t => t.Token == token);

            if (tokenRecord == null)
            {
                return new ValidateTokenResponse
                {
                    IsValid = false,
                    ErrorMessage = "Token tidak ditemukan.",
                    StatusCode = 400
                    
                };
            }

            if (tokenRecord.Revoked != null)
            {
                return new ValidateTokenResponse
                {
                    IsValid = false,
                    ErrorMessage = "Token telah dicabut.",
                    StatusCode = 401
                };
            }

            if (!tokenRecord.IsActive)
            {
                return new ValidateTokenResponse
                {
                    IsValid = false,
                    ErrorMessage = "Token telah kadaluarsa.",
                    StatusCode = 401
                };
            }

            var jwt = _configuration.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwt["SecretKey"]!);

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwt["Issuer"],
                ValidAudience = jwt["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);

            var storeId = principal.FindFirst("store_id")?.Value;
            var userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? string.Empty;
            var username = principal.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value ?? string.Empty;
            var email = principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value ?? string.Empty;
            var fullName = principal.FindFirst(JwtRegisteredClaimNames.Name)?.Value ?? string.Empty;

            var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            
            _ = Guid.TryParse(userId, out var parsedUserId) ? parsedUserId : (Guid?)null;
            _ = Guid.TryParse(storeId, out var parsedStoreId) ? parsedStoreId : (Guid?)null;

            if (!await _userRepository.IsUserAccessStoreAsync(parsedUserId, parsedStoreId))
            {
                return new ValidateTokenResponse
                {
                    IsValid = false,
                    ErrorMessage = "Tidak memiliki akses ke Toko.",
                    StatusCode = 403
                };
            }

            return new ValidateTokenResponse
            {
                IsValid = true,
                StoreId = storeId,
                UserId = userId,
                Username = username,
                FullName = fullName,
                Email = email,
                Roles = { roles },
                StatusCode = 200
            };
        }
        catch (SecurityTokenException)
        {
            Logger.Warn("Invalid token: {Token}", token);
            return new ValidateTokenResponse
            {
                IsValid = false,
                ErrorMessage = "Invalid token"
            };
        }
        catch (Exception ex)
        {
            Logger.Error($"Gagal memvalidasi token: {token}", ex);
            return new ValidateTokenResponse
            {
                IsValid = false,
                ErrorMessage = "Gagal memvalidasi token."
            };
        }
    }
}
