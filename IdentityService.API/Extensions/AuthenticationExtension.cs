using System.Text;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace IdentityService.API.Extensions;

public static class AuthenticationExtension
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        var jwt = config.GetSection("JwtSettings");
        var key = Encoding.UTF8.GetBytes(jwt["SecretKey"]!);

        services.AddAuthentication(o =>
        {
            o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(o =>
        {
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwt["Issuer"],
                ValidAudience = jwt["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };

            o.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var token = GetTokenFromRequest(context.Request);
                    if (!string.IsNullOrEmpty(token))
                        context.Token = token;

                    return Task.CompletedTask;
                },
                OnTokenValidated = async context =>
                {
                    var token = GetTokenFromRequest(context.Request);
                    
                    var db = context.HttpContext.RequestServices.GetRequiredService<IdentityDbContext>();

                    if (context.SecurityToken is not JsonWebToken)
                    {
                        context.Fail("Token tidak valid.");
                        return;
                    }

                    var tokenRecord = await db.AccessTokens.FirstOrDefaultAsync(t => t.Token == token);

                    if (tokenRecord is not { Revoked: null })
                    {
                        context.Fail("Token sudah dicabut atau tidak ditemukan.");
                        return;
                    }

                    if (!tokenRecord.IsActive)
                    {
                        context.Fail("Token sudah kedaluwarsa.");
                    }
                }
            };
        });

        return services;
    }

    private static string? GetTokenFromRequest(HttpRequest request)
    {
        // First try to get from Authorization header
        var authorization = request.Headers["Authorization"].FirstOrDefault();
        if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return authorization.Substring("Bearer ".Length).Trim();
        }

        // Then try to get from cookie
        if (request.Cookies.TryGetValue("access_token", out var cookieToken))
        {
            return cookieToken;
        }

        return null;
    }
}