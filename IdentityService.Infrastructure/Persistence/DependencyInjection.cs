using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Core.Interfaces.Services;
using IdentityService.Core.Services;
using IdentityService.Infrastructure.Repositories;
using IdentityService.Infrastructure.Services;
using ZivraFramework.EFCore.Extentions;

namespace IdentityService.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddZivraEfCore<IdentityDbContext>(options =>
        {
            options.UseNpgsql(config.GetConnectionString("DefaultConnection"));
            options.EnableSensitiveDataLogging(false);
            options.LogTo(_ => { }, Microsoft.Extensions.Logging.LogLevel.None);
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<IUserClaimRepository, UserClaimRepository>();
        services.AddScoped<IRoleClaimRepository, RoleClaimRepository>();
        services.AddScoped<IUserLoginRepository, UserLoginRepository>();
        services.AddScoped<IUserTokenRepository, UserTokenRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IPasswordHistoryRepository, PasswordHistoryRepository>();
        services.AddScoped<IUserSecurityLogRepository, UserSecurityLogRepository>();
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();
        // Branch repository & service
        services.AddScoped<IBranchRepository, BranchRepository>();
        services.AddScoped<IBranchService, BranchService>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();

        // Add Role services
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IRoleClaimService, RoleClaimService>();
        services.AddScoped<IUserClaimService, UserClaimService>();
        
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();


        return services;
    }
}