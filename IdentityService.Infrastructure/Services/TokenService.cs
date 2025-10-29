using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace IdentityService.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    #region GenerateJwtToken
    public string GenerateJwtToken(User user, IEnumerable<string?>? roles)
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

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(double.Parse(jwt["ExpireHours"] ?? "3")),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    #endregion
}
