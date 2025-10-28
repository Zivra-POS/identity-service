using System.Security.Cryptography;
using System.Text;
using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Core.Interfaces.Services;
using ZivraFramework.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace IdentityService.Infrastructure.Services;

/// <summary>
/// Layanan untuk membuat, memvalidasi, merotasi, dan mencabut refresh token.
/// Refresh token disimpan dalam bentuk hash di database; nilai mentah (raw) hanya diberikan ke klien (cookie).
/// </summary>
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

    /// <summary>
    /// Menghasilkan refresh token baru yang bersifat kriptografis aman, menyimpan versi yang di-hash ke database,
    /// dan mengembalikan nilai mentah (raw) untuk dikirim ke klien (mis. di-set sebagai cookie HttpOnly).
    /// </summary>
    /// <param name="userId">Id pengguna yang akan dikaitkan dengan refresh token.</param>
    /// <param name="ct">CancellationToken opsional.</param>
    /// <returns>Nilai mentah (raw) dari refresh token (string) yang harus diberikan ke klien.</returns>
    public async Task<string> GenerateAndStoreRefreshTokenAsync(Guid userId, CancellationToken ct = default)
    {
        var raw = GenerateRandomToken();
        var hashed = HashToken(raw);

        var refreshDays = int.TryParse(_config["JwtSettings:RefreshTokenDays"], out var days) ? days : 30;

        var rt = new RefreshToken
        {
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

    /// <summary>
    /// Memvalidasi apakah sebuah refresh token (nilai mentah) masih valid untuk seorang pengguna.
    /// </summary>
    /// <param name="userId">Id pengguna yang diklaim sebagai pemilik token.</param>
    /// <param name="token">Nilai mentah (raw) dari refresh token yang dikirim klien.</param>
    /// <param name="ct">CancellationToken opsional.</param>
    /// <returns>Id refresh token yang valid, atau null jika tidak ditemukan / tidak valid.</returns>
    public async Task<Guid?> ValidateRefreshTokenAsync(Guid userId, string token, CancellationToken ct = default)
    {
        var hashed = HashToken(token);
        var rt = await _refreshRepo.GetByUserIdAndTokenHashAsync(userId, hashed, ct);
        return rt?.Id;
    }

    /// <summary>
    /// Mencabut (revoke) refresh token berdasarkan id; menandai token sebagai telah dicabut dan menyimpan metadata revoke.
    /// </summary>
    /// <param name="id">Id refresh token yang akan dicabut.</param>
    /// <param name="revokedByIp">Opsional: alamat IP yang melakukan revoke.</param>
    /// <param name="ct">CancellationToken opsional.</param>
    /// <returns>Task yang selesai ketika operasi selesai.</returns>
    public async Task RevokeRefreshTokenAsync(Guid id, string? revokedByIp = null, CancellationToken ct = default)
    {
        var rt = await _refreshRepo.GetByIdAsync(id, ct);
        if (rt == null) return;
        rt.Revoked = DateTime.UtcNow;
        rt.RevokedByIp = revokedByIp;
        rt.ModDate = DateTime.UtcNow;
        await _refreshRepo.RevokeAsync(rt, ct);
    }

    /// <summary>
    /// Merotasi (rotate) sebuah refresh token yang ada: melakukan validasi token lama, membuat token baru,
    /// menandai token lama sebagai revoked dan menunjuk replacedByToken, menyimpan token baru, dan mengembalikan
    /// tuple (User, newRawToken) yang dapat digunakan untuk menghasilkan JWT baru dan meng-set cookie baru.
    /// Jika token tidak valid atau tidak ditemukan, mengembalikan null.
    /// </summary>
    /// <param name="existingRawToken">Nilai mentah (raw) refresh token yang dikirim klien.</param>
    /// <param name="deviceId">Opsional: device id yang akan disimpan pada token baru (jika ada).</param>
    /// <param name="ct">CancellationToken opsional.</param>
    /// <returns>
    /// Tuple berisi entitas <see cref="User"/> terkait dan nilai mentah (raw) refresh token baru;
    /// atau null jika rotasi tidak berhasil (token tidak valid/expired).
    /// </returns>
    public async Task<(User user, string newRefreshToken)?> RotateRefreshTokenAsync(string existingRawToken, string? deviceId = null, CancellationToken ct = default)
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
        
        return (user, newRaw);
    }
    
    private static string GenerateRandomToken(int size = 64)
    {
        var bytes = new byte[size];
        RandomNumberGenerator.Fill(bytes);
        // base64url
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    private static string HashToken(string token)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = sha.ComputeHash(bytes);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
