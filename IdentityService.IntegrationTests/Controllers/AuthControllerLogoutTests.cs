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

public class AuthControllerLogoutTests : BaseIntegrationTest
{
    public AuthControllerLogoutTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Logout_WithValidToken_ShouldReturnSuccess()
    {
        // Arrange
        const string password = "TestPassword123!";
        var user = await CreateTestUserAsync("testuser", "test@example.com", password, emailConfirmed: true);
        
        var loginRequest = TestDataGenerator.GenerateValidLoginRequest("testuser", password);
        var loginResponse = await PostAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.ShouldDeserializeTo<Result<AuthResponse>>();
        
        SetBearerToken(loginResult.Data!.Token);
        
        // Set refresh token cookie manually for test
        Client.DefaultRequestHeaders.Add("Cookie", $"refresh_token={loginResult.Data.RefreshToken}");

        // Act
        var response = await Client.PostAsync("/api/auth/logout", null);

        // Assert
        response.ShouldBeSuccessStatusCode();
        var result = await response.ShouldDeserializeTo<Result<string>>();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Contain("Logout berhasil");

        // Verify refresh token is revoked in database
        Context.ChangeTracker.Clear();
        var refreshTokenInDb = Context.RefreshTokens
            .AsNoTracking()
            .FirstOrDefault(rt => rt.UserId == user.Id && rt.Revoked == null);
        refreshTokenInDb.Should().BeNull();
    }

    [Fact]
    public async Task Logout_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Act
        var response = await Client.PostAsync("/api/auth/logout", null);

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_WithoutRefreshToken_ShouldReturnBadRequest()
    {
        // Arrange
        const string password = "TestPassword123!";
        var user = await CreateTestUserAsync("testuser", "test@example.com", password, emailConfirmed: true);
        
        var loginRequest = TestDataGenerator.GenerateValidLoginRequest("testuser", password);
        var loginResponse = await PostAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.ShouldDeserializeTo<Result<AuthResponse>>();
        
        SetBearerToken(loginResult.Data!.Token);
        // Don't set refresh token cookie

        // Act
        var response = await Client.PostAsync("/api/auth/logout", null);

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Refresh token tidak ditemukan");
    }

    [Fact]
    public async Task LogoutAllDevices_WithValidToken_ShouldReturnSuccess()
    {
        // Arrange
        const string password = "TestPassword123!";
        var user = await CreateTestUserAsync("testuser", "test@example.com", password, emailConfirmed: true);
        
        // Login multiple times to create multiple refresh tokens
        for (int i = 0; i < 3; i++)
        {
            var loginRequest = TestDataGenerator.GenerateValidLoginRequest("testuser", password);
            await PostAsync("/api/auth/login", loginRequest);
        }

        // Get the last login token for authentication
        var finalLoginRequest = TestDataGenerator.GenerateValidLoginRequest("testuser", password);
        var finalLoginResponse = await PostAsync("/api/auth/login", finalLoginRequest);
        var finalLoginResult = await finalLoginResponse.ShouldDeserializeTo<Result<AuthResponse>>();
        
        SetBearerToken(finalLoginResult.Data!.Token);

        // Act
        var response = await Client.PostAsync("/api/auth/logout-all-devices", null);

        // Assert
        response.ShouldBeSuccessStatusCode();
        var result = await response.ShouldDeserializeTo<Result<string>>();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Contain("Logout dari semua device berhasil");

        // Verify all refresh tokens are revoked
        var activeTokens = Context.RefreshTokens
            .Where(rt => rt.UserId == user.Id && rt.Revoked == null)
            .ToList();
        activeTokens.Should().BeEmpty();
    }

    [Fact]
    public async Task LogoutAllDevices_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Act
        var response = await Client.PostAsync("/api/auth/logout-all-devices", null);

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_ShouldCreateSecurityLog()
    {
        // Arrange
        const string password = "TestPassword123!";
        var user = await CreateTestUserAsync("testuser", "test@example.com", password, emailConfirmed: true);
        
        var loginRequest = TestDataGenerator.GenerateValidLoginRequest("testuser", password);
        var loginResponse = await PostAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.ShouldDeserializeTo<Result<AuthResponse>>();
        
        SetBearerToken(loginResult.Data!.Token);
        Client.DefaultRequestHeaders.Add("Cookie", $"refresh_token={loginResult.Data.RefreshToken}");

        // Act
        var response = await Client.PostAsync("/api/auth/logout", null);

        // Assert
        response.ShouldBeSuccessStatusCode();

        var securityLog = Context.SecurityLogs
            .Where(sl => sl.UserId == user.Id && sl.Action == "Logout")
            .FirstOrDefault();
        securityLog.Should().NotBeNull();
    }

    [Fact]
    public async Task LogoutAllDevices_ShouldCreateSecurityLog()
    {
        // Arrange
        const string password = "TestPassword123!";
        var user = await CreateTestUserAsync("testuser", "test@example.com", password, emailConfirmed: true);
        
        var loginRequest = TestDataGenerator.GenerateValidLoginRequest("testuser", password);
        var loginResponse = await PostAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.ShouldDeserializeTo<Result<AuthResponse>>();
        
        SetBearerToken(loginResult.Data!.Token);

        // Act
        var response = await Client.PostAsync("/api/auth/logout-all-devices", null);

        // Assert
        response.ShouldBeSuccessStatusCode();
        
        Context.ChangeTracker.Clear();
        var securityLog = Context.SecurityLogs
            .AsNoTracking()
            .FirstOrDefault(sl => sl.UserId == user.Id && sl.Action == "Logout Dari Semua Device");
        securityLog.Should().NotBeNull();
    }
}
