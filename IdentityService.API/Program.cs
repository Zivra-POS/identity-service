using System.Text.Json.Serialization;
using Prometheus;
using IdentityService.API.Extensions;
using IdentityService.Core.Interfaces.Services.Message;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Infrastructure.Seeder;
using IdentityService.Infrastructure.Services.Grpc;
using IdentityService.Infrastructure.Services.Message;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.OpenApi.Models;
using ZivraFramework.Core.API.Filters;
using ZivraFramework.Core.API.Middleware;
using ZivraFramework.Core.Extentions;
using ZivraFramework.Core.Interceptors;
using ZivraFramework.Core.Interfaces;
using ZivraFramework.Core.Models;
using ZivraFramework.Core.Utils;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------------------------
// CONTROLLERS (single definition only)
// ----------------------------------------------------------------------
builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ResultWrapperFilter>();
    })
    .AddJsonOptions(opt =>
    {
        // Enum as string
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

        // Prevent EF circular references
        opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// ----------------------------------------------------------------------
// Kestrel Ports
// ----------------------------------------------------------------------
builder.WebHost.ConfigureKestrel(options =>
{
    // REST API
    options.ListenAnyIP(5248, o => o.Protocols = HttpProtocols.Http1);

    // gRPC
    options.ListenAnyIP(7001, o => o.Protocols = HttpProtocols.Http2);
});

// ----------------------------------------------------------------------
// CORS
// ----------------------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ----------------------------------------------------------------------
// gRPC
// ----------------------------------------------------------------------
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

// ----------------------------------------------------------------------
// Core Services
// ----------------------------------------------------------------------
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSingleton<BaseEntityInterceptor>();
builder.Services.AddFluentValidationWithResult();

// Kafka / Events
builder.Services.AddKafkaProducer(builder.Configuration);
builder.Services.AddScoped<IUserRegisteredEvent, UserRegisteredEvent>();
builder.Services.AddScoped<IEmailVerificationEvent, EmailVerificationEvent>();

// File Helper
builder.Services.AddScoped<IFileHelper>(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    Directory.CreateDirectory(env.WebRootPath);
    return new FileHelper(env.WebRootPath);
});

// ----------------------------------------------------------------------
// Swagger
// ----------------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "IdentityService", Version = "v1" });

    // Set default server URL
    c.AddServer(new OpenApiServer { Url = "http://localhost:5248" });

    c.SupportNonNullableReferenceTypes();

    c.MapType<IFormFile>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });
});

// ----------------------------------------------------------------------
// Logging settings
// ----------------------------------------------------------------------
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.None);

var loggerOptions = builder.Configuration.GetSection("Logging").Get<LoggerOptions>();
var envFromConfig = builder.Configuration.GetValue<string>("Environment");
Logger.Configure(loggerOptions?.LogsDirectory, () => envFromConfig);

// ----------------------------------------------------------------------
// Build app
// ----------------------------------------------------------------------
var app = builder.Build();

// ----------------------------------------------------------------------
// GLOBAL EXCEPTION HANDLER (MUST BE FIRST!)
// ----------------------------------------------------------------------
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// ----------------------------------------------------------------------
// Swagger
// ----------------------------------------------------------------------
app.UseSwagger();
app.UseSwaggerUI();

// ----------------------------------------------------------------------
app.UseStaticFiles();
app.UseCors("AllowFrontend");

app.UseCookiePolicy(new CookiePolicyOptions
{
    Secure = CookieSecurePolicy.None,
    HttpOnly = HttpOnlyPolicy.None,
    MinimumSameSitePolicy = SameSiteMode.Lax
});

app.UseRouting();
app.UseHttpMetrics();

app.UseAuthentication();
app.UseAuthorization();

// ----------------------------------------------------------------------
// Endpoints
// ----------------------------------------------------------------------
app.MapControllers();
app.MapMetrics();
app.MapGrpcService<GrpcAuthService>();
app.MapGrpcReflectionService();

app.MapGet("/", () => "✅ IdentityService is running on 5248 (REST) and 7001 (gRPC)");

// ----------------------------------------------------------------------
// Seeder
// ----------------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    await RoleSeeder.SeedAsync(db);
}

// ----------------------------------------------------------------------
app.Run();

// Make Program class testable
namespace IdentityService.API
{
    public partial class Program { }
}
