using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Core.Interfaces.Services;
using IdentityService.Infrastructure.Repositories;
using IdentityService.Infrastructure.Seeder;
using IdentityService.Infrastructure.Services;
using ZivraFramework.Core.Interceptors;
using ZivraFramework.Core.Interfaces;
using ZivraFramework.EFCore.Extentions;
using ZivraFramework.EFCore.Repositories;

namespace IdentityService.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<IdentityDbContext>((sp, options) =>
        {
            options.UseNpgsql(config.GetConnectionString("DefaultConnection"));
            options.AddInterceptors(sp.GetRequiredService<BaseEntityInterceptor>());
            options.EnableSensitiveDataLogging(false);
            options.LogTo(_ => { }, Microsoft.Extensions.Logging.LogLevel.None);
        });
        
        services.AddScoped<IUnitOfWork, UnitOfWork<IdentityDbContext>>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<IUserClaimRepository, UserClaimRepository>();
        services.AddScoped<IRoleClaimRepository, RoleClaimRepository>();
        services.AddScoped<IUserLoginRepository, UserLoginRepository>();
        services.AddScoped<IUserTokenRepository, UserTokenRepository>();
        services.AddScoped<IAccessTokenRepository, AccessTokenRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IPasswordHistoryRepository, PasswordHistoryRepository>();
        services.AddScoped<IUserSecurityLogRepository, UserSecurityLogRepository>();
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();
        services.AddScoped<IBranchRepository, BranchRepository>();
        services.AddScoped<IStoreRepository, StoreRepository>();
        services.AddScoped<IUserBranchRepository, UserBranchRepository>();
        
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<IUserBranchService, UserBranchService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IRoleClaimService, RoleClaimService>();
        services.AddScoped<IUserClaimService, UserClaimService>();
        services.AddScoped<IUserRoleService, UserRoleService>();
        
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        
        return services;
    }
}