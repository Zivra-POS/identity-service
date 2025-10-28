using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using IdentityService.Core.Interfaces.Services.Message;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Infrastructure.Services.Message;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ZivraFramework.Core.Extentions;
using ZivraFramework.Core.Interfaces;
using ZivraFramework.Core.Models;
using ZivraFramework.Core.Utils;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.None);

var validatorAssemblies = AppDomain.CurrentDomain.GetAssemblies()
    .Where(a => !a.IsDynamic && a.GetName().Name != null && a.GetName().Name!.StartsWith("IdentityService"))
    .ToArray();

builder.Services.AddValidatorsFromAssemblies(validatorAssemblies);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);

var loggerOptions = builder.Configuration.GetSection("Logging").Get<LoggerOptions>();
var envFromConfig = builder.Configuration.GetValue<string>("Environment");
Logger.Configure(logsDirectory: loggerOptions?.LogsDirectory, environmentGetter: () => envFromConfig);

var jwt = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwt["SecretKey"]!);

builder.Services.AddAuthentication(o =>
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
                {
                    Console.WriteLine($"✅ Token dari Cookie terbaca: {token.Substring(0, 20)}...");
                    context.Token = token;
                }
                else
                {
                    Console.WriteLine("❌ Cookie 'access_token' tidak ditemukan di request.");
                }
                return Task.CompletedTask;
            }
        };
    });


// File helper
builder.Services.AddScoped<IFileHelper>(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    var webRoot = env.WebRootPath;
    Directory.CreateDirectory(webRoot);
    return new FileHelper(webRoot);
});

// Kafka
builder.Services.AddKafkaProducer(builder.Configuration);
builder.Services.AddScoped<IUserRegisteredEvent, UserRegisteredEvent>();
builder.Services.AddScoped<IEmailVerificationEvent, EmailVerificationEvent>();

Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"Kafka BootstrapServers: {builder.Configuration["Kafka:BootstrapServers"]}");



var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();