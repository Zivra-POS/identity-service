using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using IdentityService.IntegrationTests.Infrastructure;
using IdentityService.Core.Entities;
using IdentityService.Infrastructure.Persistence;
using Xunit;

namespace IdentityService.IntegrationTests.Base;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    protected readonly IntegrationTestWebAppFactory Factory;
    protected readonly HttpClient Client;
    protected readonly IServiceScope Scope;
    protected readonly IdentityDbContext Context;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        Scope = factory.Services.CreateScope();
        Context = Scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    }

    public virtual Task InitializeAsync() => Task.CompletedTask;

    public virtual async Task DisposeAsync()
    {
        try
        {
            await Factory.ResetDatabaseAsync();
        }
        catch (Exception ex)
        {
            // Don't fail tests due to teardown errors; write to console for diagnosis
            Console.Error.WriteLine($"Warning: ResetDatabaseAsync failed during DisposeAsync: {ex}");
        }
        finally
        {
            Scope.Dispose();
            Client.Dispose();
        }
    }

    protected async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data)
    {
        var json = JsonSerializer.Serialize(data, JsonSerializerOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await Client.PostAsync(endpoint, content);
    }

    protected async Task<HttpResponseMessage> PutAsync<T>(string endpoint, T data)
    {
        var json = JsonSerializer.Serialize(data, JsonSerializerOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await Client.PutAsync(endpoint, content);
    }

    protected async Task<HttpResponseMessage> PostFormDataAsync(string endpoint, MultipartFormDataContent content)
    {
        return await Client.PostAsync(endpoint, content);
    }

    protected async Task<HttpResponseMessage> PutFormDataAsync(string endpoint, MultipartFormDataContent content)
    {
        return await Client.PutAsync(endpoint, content);
    }

    protected void SetBearerToken(string token)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    protected void ClearAuthorizationHeader()
    {
        Client.DefaultRequestHeaders.Authorization = null;
    }

    protected async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, JsonSerializerOptions);
    }

    protected static JsonSerializerOptions JsonSerializerOptions => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    protected async Task<User> CreateTestUserAsync(
        string username = "testuser",
        string email = "test@example.com",
        string password = "TestPassword123!",
        bool emailConfirmed = true,
        string? profileUrl = null,
        string? displayName = null,
        string? phoneNumber = null,
        Guid? storeId = null,
        Guid? ownerId = null)
    {
        const int maxAttempts = 5;
        var baseUsername = username;
        var baseEmail = email;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var candidateUsername = attempt == 1 ? baseUsername : $"{baseUsername}_{Guid.NewGuid().ToString("N").Substring(0,8)}";
            var candidateEmail = attempt == 1 ? baseEmail : $"{Path.GetFileNameWithoutExtension(baseEmail)}+{Guid.NewGuid().ToString("N").Substring(0,8)}@{baseEmail.Split('@').Last()}";

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = "Test User",
                Username = candidateUsername,
                NormalizedUsername = candidateUsername.ToUpperInvariant(),
                Email = candidateEmail,
                NormalizedEmail = candidateEmail.ToUpperInvariant(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                EmailConfirmed = emailConfirmed,
                IsActive = true,
                ProfileUrl = profileUrl,
                DisplayName = displayName,
                PhoneNumber = phoneNumber,
                StoreId = storeId,
                OwnerId = ownerId,
                IsFirstLogin = false,
                CreDate = DateTime.UtcNow,
                CreBy = "SYSTEM",
                CreIpAddress = "127.0.0.1"
            };

            Context.Users.Add(user);
            try
            {
                await Context.SaveChangesAsync();
                return user;
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                // Handle unique constraint violation (duplicate username/email). If this happens,
                // try again with a different username/email. If other error, rethrow.
                if (dbEx.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505")
                {
                    // duplicate key, try another candidate
                    Context.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                    if (attempt == maxAttempts) throw;
                    await Task.Delay(50 * attempt);
                    continue;
                }

                // Not a duplicate-key postgres error, rethrow
                throw;
            }
        }

        throw new InvalidOperationException("Failed to create test user after multiple attempts.");
    }

    protected async Task<Role> CreateTestRoleAsync(string roleName = "TestRole")
    {
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = roleName,
            NormalizedName = roleName.ToUpperInvariant(),
            CreDate = DateTime.UtcNow,
            CreBy = "SYSTEM",
            CreIpAddress = "127.0.0.1"
        };

        Context.Roles.Add(role);
        await Context.SaveChangesAsync();
        return role;
    }

    protected async Task AssignRoleToUserAsync(Guid userId, Guid roleId)
    {
        // Ensure role exists to avoid FK constraint errors in tests
        var role = await Context.Roles.FindAsync(roleId);
        if (role == null)
        {
            role = new Role
            {
                Id = roleId,
                Name = $"Role_{roleId.ToString().Substring(0,8)}",
                NormalizedName = ($"Role_{roleId.ToString().Substring(0,8)}").ToUpperInvariant(),
                CreDate = DateTime.UtcNow,
                CreBy = "SYSTEM",
                CreIpAddress = "127.0.0.1"
            };
            Context.Roles.Add(role);
            await Context.SaveChangesAsync();
        }

        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            CreDate = DateTime.UtcNow,
            CreBy = "SYSTEM",
            CreIpAddress = "127.0.0.1"
        };

        Context.UserRoles.Add(userRole);
        await Context.SaveChangesAsync();
    }

    protected async Task<Store> CreateTestStoreAsync(string name = "Test Store")
    {
        var store = new Store
        {
            Id = Guid.NewGuid(),
            Name = name,
            Code = null,
            Address = "Test Address",
            Province = "Test Province",
            City = "Test City",
            District = "Test District",
            Rt = "01",
            Rw = "01",
            Phone = "081234567890",
            IsActive = true,
            CreDate = DateTime.UtcNow,
            CreBy = "SYSTEM",
            CreIpAddress = "127.0.0.1"
        };

        Context.Stores.Add(store);
        await Context.SaveChangesAsync();
        return store;
    }

}
