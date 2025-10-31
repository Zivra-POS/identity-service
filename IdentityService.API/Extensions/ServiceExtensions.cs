using IdentityService.API.Middleware;
using IdentityService.API.Filters;
using FluentValidation;
using System.Reflection;

namespace IdentityService.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddGlobalExceptionHandler(this IServiceCollection services)
    {
        // Add Result wrapper filter to automatically wrap responses in Result<T>
        services.AddControllers(options =>
        {
            options.Filters.Add<ResultWrapperFilter>();
        });
        
        return services;
    }

    public static IServiceCollection AddFluentValidationWithResult(this IServiceCollection services, Assembly assembly)
    {
        // Register FluentValidation
        services.AddValidatorsFromAssembly(assembly);
        
        // Register MediatR with validation behavior (optional, jika menggunakan MediatR)
        // services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        // services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        
        return services;
    }

    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        return app;
    }
}

