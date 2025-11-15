using IdentityService.API.Filters;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using ZivraFramework.Core.API.Filters;
using ZivraFramework.Core.API.Middleware;

namespace IdentityService.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddGlobalExceptionHandler(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add<ResultWrapperFilter>();
            options.Filters.Add<ModelValidationActionFilter>();
        });
        
        return services;
    }

    public static IServiceCollection AddFluentValidationWithResult(this IServiceCollection services)
    {
        var validatorAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && 
                       a.GetName().Name != null && 
                       a.GetName().Name.StartsWith("IdentityService"))
            .ToArray();

        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();
        services.AddValidatorsFromAssemblies(validatorAssemblies);
        
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });
        
        return services;
    }

    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        return app;
    }
}

