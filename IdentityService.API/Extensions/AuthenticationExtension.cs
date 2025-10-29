using System.IdentityModel.Tokens.Jwt;
using System.Text;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace IdentityService.API.Extensions;

public static class AuthenticationExtension
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        var jwt = config.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwt["Key"]!);

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
                    if (context.Request.Cookies.TryGetValue("access_token", out var token))
                        context.Token = token;

                    return Task.CompletedTask;
                },
                OnTokenValidated = async context =>
                {
                    var db = context.HttpContext.RequestServices.GetRequiredService<IdentityDbContext>();

                    if (context.SecurityToken is not JwtSecurityToken jwtToken)
                    {
                        context.Fail("Token tidak valid.");
                        return;
                    }

                    var raw = jwtToken.RawData;
                    var tokenRecord = await db.AccessTokens.FirstOrDefaultAsync(t => t.Token == raw);

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
}