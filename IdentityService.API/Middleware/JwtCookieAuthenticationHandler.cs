using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace IdentityService.API.Middleware;

public class JwtCookieAuthenticationHandler : AuthenticationHandler<JwtBearerOptions>
{
    public JwtCookieAuthenticationHandler(
        IOptionsMonitor<JwtBearerOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var token = GetTokenFromRequest();

        if (string.IsNullOrEmpty(token))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = Options.TokenValidationParameters;

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Token validation failed");
            return Task.FromResult(AuthenticateResult.Fail("Token validation failed"));
        }
    }

    private string? GetTokenFromRequest()
    {
        // First try to get access token from Authorization header
        var authorization = Request.Headers["Authorization"].FirstOrDefault();
        if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return authorization.Substring("Bearer ".Length).Trim();
        }

        // Try to get access token from custom header
        if (Request.Headers.TryGetValue("X-Access-Token", out var accessTokenHeader))
        {
            var headerToken = accessTokenHeader.FirstOrDefault();
            if (!string.IsNullOrEmpty(headerToken))
            {
                return headerToken;
            }
        }

        // Try to get access token from cookie
        if (Request.Cookies.TryGetValue("access_token", out var accessTokenCookie))
        {
            return accessTokenCookie;
        }

        // If no access token found, try refresh token from header
        if (Request.Headers.TryGetValue("X-Refresh-Token", out var refreshTokenHeader))
        {
            var headerRefreshToken = refreshTokenHeader.FirstOrDefault();
            if (!string.IsNullOrEmpty(headerRefreshToken))
            {
                // For refresh token, we might need different handling
                // But for now, we'll return it as is for validation
                return headerRefreshToken;
            }
        }

        // Try to get refresh token from cookie as fallback
        if (Request.Cookies.TryGetValue("refresh_token", out var refreshTokenCookie))
        {
            // For refresh token, we might need different handling
            // But for now, we'll return it as is for validation
            return refreshTokenCookie;
        }

        return null;
    }
}
