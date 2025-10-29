using Grpc.Core;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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

    public GrpcAuthService(
        IdentityDbContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
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
                    ErrorMessage = "Token tidak ditemukan."
                };
            }

            if (tokenRecord.Revoked != null)
            {
                return new ValidateTokenResponse
                {
                    IsValid = false,
                    ErrorMessage = "Token telah dicabut."
                };
            }

            if (!tokenRecord.IsActive)
            {
                return new ValidateTokenResponse
                {
                    IsValid = false,
                    ErrorMessage = "Token telah kadaluarsa."
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

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var username = principal.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
            var email = principal.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
            var fullName = principal.FindFirst("FullName")?.Value ?? string.Empty;
            
            var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            return new ValidateTokenResponse
            {
                IsValid = true,
                UserId = userId,
                Username = username,
                FullName = fullName,
                Email = email,
                Roles = { roles }
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
