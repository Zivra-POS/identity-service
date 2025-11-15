using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ZivraFramework.Core.Interfaces;

namespace IdentityService.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly IAccessTokenRepository _accessTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TokenService(IConfiguration config, IAccessTokenRepository accessTokenRepository, IUnitOfWork unitOfWork)
    {
        _config = config;
        _accessTokenRepository = accessTokenRepository;
        _unitOfWork = unitOfWork;
    }

    #region GenerateJwtToken
    public async Task<AccessToken> GenerateJwtToken(User user, IEnumerable<string?>? roles)
    {
        var jwt = _config.GetSection("JwtSettings");
        var secret = jwt["SecretKey"];
        if (string.IsNullOrEmpty(secret))
            throw new InvalidOperationException("JwtSettings:SecretKey is not configured.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString())
        };

        if (!string.IsNullOrWhiteSpace(user.Username))
            claims.Add(new Claim(JwtRegisteredClaimNames.UniqueName, user.Username));

        if (!string.IsNullOrWhiteSpace(user.Email))
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));

        if (!string.IsNullOrWhiteSpace(user.FullName))
            claims.Add(new Claim(JwtRegisteredClaimNames.Name, user.FullName));

        claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

        var safeRoles = roles ?? Enumerable.Empty<string?>();
        var roleClaims = safeRoles
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => new Claim(ClaimTypes.Role, r!));

        claims.AddRange(roleClaims);
        claims.Add(new Claim("is_active", user.IsActive.ToString()));
        
        if (!string.IsNullOrEmpty(user.HashedStoreId))
            claims.Add(new Claim("store_id", user.HashedStoreId ?? string.Empty));

        var expires = DateTime.UtcNow.AddHours(double.Parse(jwt["ExpireHours"] ?? "3"));
        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: creds
        );

        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

        var accessToken = new AccessToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id ,
            Token = jwtToken,
            Expires = expires,
            Revoked = null,
            RevokedByIp = null,
            CreDate = DateTime.UtcNow,
            CreBy = user.CreBy,
            CreIpAddress = user.CreIpAddress,
            ModDate = null,
            ModBy = null,
            ModIpAddress = null
        };
        
        await _accessTokenRepository.AddAsync(accessToken);
        await _unitOfWork.SaveChangesAsync();
        return accessToken;
    }
    #endregion

    #region RevokeJwtTokenAsync
    public async Task RevokeJwtTokenAsync(Guid accessTokenId)
    {
        await _accessTokenRepository.RevokeAsync(accessTokenId);
        await _unitOfWork.SaveChangesAsync();
    }
    #endregion

    #region IsJwtTokenRevokedAsync
    public async Task<bool> IsJwtTokenRevokedAsync(Guid accessTokenId)
    {
        return await _accessTokenRepository.IsRevokedAsync(accessTokenId);
    }
    #endregion

    #region RevokeAllAccessTokensForUserAsync
    public async Task RevokeAllAccessTokensForUserAsync(Guid userId, string? revokedByIp = null, CancellationToken ct = default)
    {
        var tokens = await _accessTokenRepository.GetActiveByUserIdAsync(userId, ct);
        if (tokens == null) return;

        foreach (var at in tokens)
        {
            await _accessTokenRepository.RevokeAsync(at.Id, revokedByIp, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }
    #endregion
}
