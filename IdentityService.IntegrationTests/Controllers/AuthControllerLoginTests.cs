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

public class AuthControllerLoginTests : BaseIntegrationTest
{
    public AuthControllerLoginTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnSuccess()
    {
        const string password = "TestPassword123!";
        var user = await CreateTestUserAsync("testuser", "test@example.com", password, emailConfirmed: true);
        var role = await CreateTestRoleAsync("TestRole");
        await AssignRoleToUserAsync(user.Id, role.Id);

        var loginRequest = TestDataGenerator.GenerateValidLoginRequest("testuser", password);

        var response = await PostAsync("/api/auth/login", loginRequest);

        response.ShouldBeSuccessStatusCode();
        response.ShouldHaveCookie("access_token");
        response.ShouldHaveCookie("refresh_token");

        var result = await response.ShouldDeserializeTo<Result<AuthResponse>>();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Username.Should().Be("testuser");
        result.Data.Token.Should().NotBeNullOrEmpty();
        result.Data.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidUsername_ShouldReturnNotFound()
    {
        var loginRequest = TestDataGenerator.GenerateValidLoginRequest("nonexistentuser", "Password123!");

        var response = await PostAsync("/api/auth/login", loginRequest);

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Username tidak ditemukan");
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnBadRequest()
    {
        var user = await CreateTestUserAsync("testuser", "test@example.com", "CorrectPassword123!", emailConfirmed: true);
        var loginRequest = TestDataGenerator.GenerateValidLoginRequest("testuser", "WrongPassword123!");

        var response = await PostAsync("/api/auth/login", loginRequest);

        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Password salah");
    }

    [Fact]
    public async Task Login_WithUnverifiedEmail_ShouldReturnForbidden()
    {
        const string password = "TestPassword123!";
        var user = await CreateTestUserAsync("testuser", "test@example.com", password, emailConfirmed: false);
        var loginRequest = TestDataGenerator.GenerateValidLoginRequest("testuser", password);

        var response = await PostAsync("/api/auth/login", loginRequest);

        response.ShouldHaveStatusCode(HttpStatusCode.Forbidden);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Email belum terverifikasi");
    }

    [Fact]
    public async Task Login_WithLockedAccount_ShouldReturnLocked()
    {
        const string password = "TestPassword123!";
        var user = await CreateTestUserAsync("testuser", "test@example.com", password, emailConfirmed: true);
        
        user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(30);
        user.AccessFailedCount = 5;
        Context.Users.Update(user);
        await Context.SaveChangesAsync();

        var loginRequest = TestDataGenerator.GenerateValidLoginRequest("testuser", password);

        var response = await PostAsync("/api/auth/login", loginRequest);

        response.ShouldHaveStatusCode(HttpStatusCode.Locked);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Akun terkunci");
    }

    [Fact]
    public async Task Login_MultipleFailedAttempts_ShouldLockAccount()
    {
        const string password = "TestPassword123!";
        var user = await CreateTestUserAsync("testuser", "test@example.com", password, emailConfirmed: true);
        var loginRequest = TestDataGenerator.GenerateValidLoginRequest("testuser", "WrongPassword!");

        for (int i = 0; i < 5; i++)
        {
            await PostAsync("/api/auth/login", loginRequest);
        }

        var finalResponse = await PostAsync("/api/auth/login", loginRequest);

        finalResponse.ShouldHaveStatusCode(HttpStatusCode.Locked);
        var content = await finalResponse.Content.ReadAsStringAsync();
        content.Should().Contain("Akun terkunci");

        Context.ChangeTracker.Clear();
        var userInDb = Context.Users.AsNoTracking().First(u => u.Id == user.Id);
        userInDb.AccessFailedCount.Should().Be(5);
        userInDb.LockoutEnd.Should().BeAfter(DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Login_ShouldResetFailedAttemptsOnSuccess()
    {
        const string password = "TestPassword123!";
        var user = await CreateTestUserAsync("testuser", "test@example.com", password, emailConfirmed: true);
        
        user.AccessFailedCount = 3;
        Context.Users.Update(user);
        await Context.SaveChangesAsync();

        var loginRequest = TestDataGenerator.GenerateValidLoginRequest("testuser", password);

        var response = await PostAsync("/api/auth/login", loginRequest);

        response.ShouldBeSuccessStatusCode();

        Context.ChangeTracker.Clear();
        var userInDb = Context.Users.AsNoTracking().First(u => u.Id == user.Id);
        userInDb.AccessFailedCount.Should().Be(0);
        userInDb.LockoutEnd.Should().BeNull();
    }

    [Fact]
    public async Task Login_ShouldCreateSecurityLog()
    {
        const string password = "TestPassword123!";
        var user = await CreateTestUserAsync("testuser", "test@example.com", password, emailConfirmed: true);
        var loginRequest = TestDataGenerator.GenerateValidLoginRequest("testuser", password);

        var response = await PostAsync("/api/auth/login", loginRequest);

        response.ShouldBeSuccessStatusCode();

        var securityLog = Context.SecurityLogs
            .Where(sl => sl.UserId == user.Id && sl.Description.Contains("login berhasil"))
            .FirstOrDefault();
        securityLog.Should().NotBeNull();
    }

    [Theory]
    [InlineData("", "Password123!")]
    [InlineData("testuser", "")]
    public async Task Login_WithMissingFields_ShouldReturnBadRequest(string username, string password)
    {
        var loginRequest = TestDataGenerator.GenerateValidLoginRequest(username, password);

        var response = await PostAsync("/api/auth/login", loginRequest);

        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }
}
