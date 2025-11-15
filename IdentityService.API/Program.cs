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
using ZivraFramework.Core.Extentions;
using ZivraFramework.Core.Interceptors;
using ZivraFramework.Core.Interfaces;
using ZivraFramework.Core.Models;
using ZivraFramework.Core.Utils;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

//builder.WebHost.ConfigureKestrel(options =>
// {
   // options.ListenAnyIP(7001, o => o.Protocols = HttpProtocols.Http2);
    //options.ListenAnyIP(5001, o => o.Protocols = HttpProtocols.Http1);
//});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Remove(
            o.JsonSerializerOptions.Converters
                .FirstOrDefault(c => c is JsonStringEnumConverter)
        );
    });



// Add gRPC services
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

builder.Services.AddSingleton<BaseEntityInterceptor>();


// Add FluentValidation with automatic Result wrapper integration
builder.Services.AddFluentValidationWithResult();

// Add Global Exception Handler with Result wrapper
builder.Services.AddGlobalExceptionHandler();

builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.None);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "IdentityService", Version = "v1" });
    c.AddServer(new OpenApiServer
    {
        Url = "http://localhost:5248"
    });
    c.SupportNonNullableReferenceTypes();
    c.MapType<IFormFile>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });
});


builder.Services.AddInfrastructure(builder.Configuration);

var loggerOptions = builder.Configuration.GetSection("Logging").Get<LoggerOptions>();
var envFromConfig = builder.Configuration.GetValue<string>("Environment");
Logger.Configure(logsDirectory: loggerOptions?.LogsDirectory, environmentGetter: () => envFromConfig);

builder.Services.AddJwtAuthentication(builder.Configuration);

// File helper
builder.Services.AddScoped<IFileHelper>(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    var webRoot = env.WebRootPath;
    Directory.CreateDirectory(webRoot);
    return new FileHelper(webRoot);
});

// JSON Enum Converter
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Kafka
builder.Services.AddKafkaProducer(builder.Configuration);
builder.Services.AddScoped<IUserRegisteredEvent, UserRegisteredEvent>();
builder.Services.AddScoped<IEmailVerificationEvent, EmailVerificationEvent>();

builder.Services.AddScoped<GrpcAuthService>();

Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"Kafka BootstrapServers: {builder.Configuration["Kafka:BootstrapServers"]}");


var app = builder.Build();

// Use Global Exception Handler 
app.UseGlobalExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI();

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

app.MapControllers();
app.MapMetrics();

app.MapGrpcService<GrpcAuthService>();
app.MapGrpcReflectionService(); 
app.MapGet("/", () => "✅ IdentityService is running. gRPC:7001");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    await RoleSeeder.SeedAsync(db);
}

app.Run();

// Make the Program class accessible for testing
namespace IdentityService.API
{
    public partial class Program { }
}

