using System.Security.Cryptography;
using System.Text;
using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Core.Interfaces.Services;
using ZivraFramework.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace IdentityService.Infrastructure.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IRefreshTokenRepository _refreshRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _config;

    public RefreshTokenService(IRefreshTokenRepository refreshRepo, IUnitOfWork unitOfWork, IConfiguration config)
    {
        _refreshRepo = refreshRepo;
        _unitOfWork = unitOfWork;
        _config = config;
    }

    #region GenerateAndStoreRefreshTokenAsync
    public async Task<string> GenerateAndStoreRefreshTokenAsync(Guid userId, Guid accessTokenId, CancellationToken ct = default)
    {
        var raw = GenerateRandomToken();
        var hashed = HashToken(raw);

        var refreshDays = int.TryParse(_config["JwtSettings:RefreshTokenDays"], out var days) ? days : 30;

        var rt = new RefreshToken
        {
            AccessTokenId = accessTokenId,
            UserId = userId,
            Token = hashed,
            Expires = DateTime.UtcNow.AddDays(refreshDays),
            CreDate = DateTime.UtcNow,
            CreIpAddress = null,
        };

        await _refreshRepo.AddAsync(rt, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return raw;
    }
    #endregion

    #region ValidateRefreshTokenAsync
    public async Task<Guid?> ValidateRefreshTokenAsync(Guid userId, string token, CancellationToken ct = default)
    {
        var hashed = HashToken(token);
        var rt = await _refreshRepo.GetByUserIdAndTokenHashAsync(userId, hashed, ct);
        return rt?.Id;
    }
    #endregion

    #region RevokeRefreshTokenAsync
    public async Task RevokeRefreshTokenAsync(Guid id, string? revokedByIp = null, CancellationToken ct = default)
    {
        var rt = await _refreshRepo.GetByIdAsync(id, ct);
        if (rt == null) return;

        rt.Revoked = DateTime.UtcNow;
        rt.RevokedByIp = revokedByIp;
        rt.ModDate = DateTime.UtcNow;

        await _refreshRepo.RevokeAsync(rt, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
    #endregion

    #region RotateRefreshTokenAsync
    public async Task<string?> RotateRefreshTokenAsync(string existingRawToken, Guid accessTokenId, string? deviceId = null, CancellationToken ct = default)
    {
        var existingHash = HashToken(existingRawToken);
        var existing = await _refreshRepo.GetByTokenHashAsync(existingHash, ct);
        if (existing == null) return null;

        var user = existing.User;
        if (user == null) return null;

        var newRaw = GenerateRandomToken();
        var newHash = HashToken(newRaw);
        var refreshDays = int.TryParse(_config["JwtSettings:RefreshTokenDays"], out var days) ? days : 30;

        var newRt = new RefreshToken
        {
            AccessTokenId = accessTokenId,
            UserId = user.Id,
            Token = newHash,
            Expires = DateTime.UtcNow.AddDays(refreshDays),
            CreDate = DateTime.UtcNow,
            CreIpAddress = null,
            DeviceId = deviceId ?? existing.DeviceId
        };

        existing.Revoked = DateTime.UtcNow;
        existing.RevokedByIp = null;
        existing.ReplacedByToken = newHash;
        existing.ModDate = DateTime.UtcNow;

        await _refreshRepo.AddAsync(newRt, ct);
        await _refreshRepo.RevokeAsync(existing, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return newRaw;
    }
    #endregion

    #region GenerateRandomToken
    private static string GenerateRandomToken(int size = 64)
    {
        var bytes = new byte[size];
        RandomNumberGenerator.Fill(bytes);

        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
    #endregion

    #region HashToken
    private static string HashToken(string token)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = sha.ComputeHash(bytes);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
    #endregion
}
