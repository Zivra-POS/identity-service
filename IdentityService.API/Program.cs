using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using IdentityService.API.Extensions;
using IdentityService.Core.Interfaces.Services.Message;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Infrastructure.Services.Grpc;
using IdentityService.Infrastructure.Services.Message;
using ZivraFramework.Core.Extentions;
using ZivraFramework.Core.Interfaces;
using ZivraFramework.Core.Models;
using ZivraFramework.Core.Utils;
using System.Reflection;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Add gRPC services
builder.Services.AddGrpc();

// Keep existing FluentValidation setup
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

// Add validators
var validatorAssemblies = AppDomain.CurrentDomain.GetAssemblies()
    .Where(a => !a.IsDynamic && a.GetName().Name != null && a.GetName().Name!.StartsWith("IdentityService"))
    .ToArray();

builder.Services.AddValidatorsFromAssemblies(validatorAssemblies);

// Add Global Exception Handler with Result wrapper
builder.Services.AddGlobalExceptionHandler();

builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.None);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// Use Global Exception Handler (must be first in pipeline)
app.UseGlobalExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGrpcService<GrpcAuthService>();

app.Run();