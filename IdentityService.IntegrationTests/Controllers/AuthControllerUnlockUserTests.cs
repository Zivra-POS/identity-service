using System.Net;
using FluentAssertions;
using IdentityService.IntegrationTests.Base;
using IdentityService.IntegrationTests.Helpers;
using IdentityService.IntegrationTests.Infrastructure;
using IdentityService.Shared.DTOs.Response.Auth;
using IdentityService.Shared.Response;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IdentityService.IntegrationTests.Controllers;

public class AuthControllerUnlockUserTests : BaseIntegrationTest
{
    public AuthControllerUnlockUserTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UnlockUser_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        const string password = "TestPassword123!";
        var adminUser = await CreateTestUserAsync("admin", "admin@example.com", password, emailConfirmed: true);
        var lockedUser = await CreateTestUserAsync("lockeduser", "locked@example.com", password, emailConfirmed: true);
        
        // Lock the user
        lockedUser.AccessFailedCount = 5;
        lockedUser.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(30);
        Context.Users.Update(lockedUser);
        await Context.SaveChangesAsync();

        // Login as admin
        var loginRequest = TestDataGenerator.GenerateValidLoginRequest("admin", password);
        var loginResponse = await PostAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.ShouldDeserializeTo<Result<AuthResponse>>();
        SetBearerToken(loginResult.Data!.Token);

        var unlockUserRequest = TestDataGenerator.GenerateValidUnlockUserRequest(lockedUser.Id);

        // Act
        var response = await PostAsync("/api/auth/unlock-user", unlockUserRequest);

        // Assert
        response.ShouldBeSuccessStatusCode();
        var result = await response.ShouldDeserializeTo<Result<string>>();
        
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Contain("User berhasil dibuka dari lockout");

        // Verify user is unlocked in database
        Context.ChangeTracker.Clear();
        var userInDb = await Context.Users.AsNoTracking().FirstAsync(u => u.Id == lockedUser.Id);
        userInDb.AccessFailedCount.Should().Be(0);
        userInDb.LockoutEnd.Should().BeNull();
    }

    [Fact]
    public async Task UnlockUser_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        var unlockUserRequest = TestDataGenerator.GenerateValidUnlockUserRequest(Guid.NewGuid());

        // Act
        var response = await PostAsync("/api/auth/unlock-user", unlockUserRequest);

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UnlockUser_WithNonExistentUser_ShouldReturnNotFound()
    {
        // Arrange
        const string password = "TestPassword123!";
        var adminUser = await CreateTestUserAsync("admin", "admin@example.com", password, emailConfirmed: true);
        
        var loginRequest = TestDataGenerator.GenerateValidLoginRequest("admin", password);
        var loginResponse = await PostAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.ShouldDeserializeTo<Result<AuthResponse>>();
        SetBearerToken(loginResult.Data!.Token);

        var unlockUserRequest = TestDataGenerator.GenerateValidUnlockUserRequest(Guid.NewGuid()); // Non-existent user

        // Act
        var response = await PostAsync("/api/auth/unlock-user", unlockUserRequest);

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("User tidak ditemukan");
    }

    [Fact]
    public async Task UnlockUser_WithAlreadyUnlockedUser_ShouldReturnBadRequest()
    {
        // Arrange
        const string password = "TestPassword123!";
        var adminUser = await CreateTestUserAsync("admin", "admin@example.com", password, emailConfirmed: true);
        var unlockedUser = await CreateTestUserAsync("unlockeduser", "unlocked@example.com", password, emailConfirmed: true);
        
        var loginRequest = TestDataGenerator.GenerateValidLoginRequest("admin", password);
        var loginResponse = await PostAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.ShouldDeserializeTo<Result<AuthResponse>>();
        SetBearerToken(loginResult.Data!.Token);

        var unlockUserRequest = TestDataGenerator.GenerateValidUnlockUserRequest(unlockedUser.Id);

        // Act
        var response = await PostAsync("/api/auth/unlock-user", unlockUserRequest);

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("User tidak dalam keadaan terkunci");
    }

    [Fact]
    public async Task UnlockUser_ShouldCreateSecurityLog()
    {
        // Arrange
        const string password = "TestPassword123!";
        var adminUser = await CreateTestUserAsync("admin", "admin@example.com", password, emailConfirmed: true);
        var lockedUser = await CreateTestUserAsync("lockeduser", "locked@example.com", password, emailConfirmed: true);
        
        // Lock the user
        lockedUser.AccessFailedCount = 5;
        lockedUser.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(30);
        Context.Users.Update(lockedUser);
        await Context.SaveChangesAsync();

        var loginRequest = TestDataGenerator.GenerateValidLoginRequest("admin", password);
        var loginResponse = await PostAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.ShouldDeserializeTo<Result<AuthResponse>>();
        SetBearerToken(loginResult.Data!.Token);

        var unlockUserRequest = TestDataGenerator.GenerateValidUnlockUserRequest(lockedUser.Id);

        // Act
        var response = await PostAsync("/api/auth/unlock-user", unlockUserRequest);

        // Assert
        response.ShouldBeSuccessStatusCode();
        
        Context.ChangeTracker.Clear();
        
        var securityLog = Context.SecurityLogs
            .AsNoTracking()
            .FirstOrDefault(sl => sl.UserId == lockedUser.Id && sl.Action == "Akun Dibuka Kuncinya");
        securityLog.Should().NotBeNull();
    }

    [Fact]
    public async Task UnlockUser_WithReason_ShouldIncludeReasonInSecurityLog()
    {
        // Arrange
        const string password = "TestPassword123!";
        var adminUser = await CreateTestUserAsync("admin", "admin@example.com", password, emailConfirmed: true);
        var lockedUser = await CreateTestUserAsync("lockeduser", "locked@example.com", password, emailConfirmed: true);
        
        // Lock the user
        lockedUser.AccessFailedCount = 5;
        lockedUser.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(30);
        Context.Users.Update(lockedUser);
        await Context.SaveChangesAsync();

        var loginRequest = TestDataGenerator.GenerateValidLoginRequest("admin", password);
        var loginResponse = await PostAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.ShouldDeserializeTo<Result<AuthResponse>>();
        SetBearerToken(loginResult.Data!.Token);

        var unlockUserRequest = TestDataGenerator.GenerateValidUnlockUserRequest(lockedUser.Id);
        unlockUserRequest.Reason = "Customer support request";

        // Act
        var response = await PostAsync("/api/auth/unlock-user", unlockUserRequest);

        // Assert
        response.ShouldBeSuccessStatusCode();

        var securityLog = Context.SecurityLogs
            .FirstOrDefault(sl => sl.UserId == lockedUser.Id && sl.Action == "Akun Dibuka Kuncinya");
        securityLog.Should().NotBeNull();
        securityLog!.Description.Should().Contain("Customer support request");
    }
}
