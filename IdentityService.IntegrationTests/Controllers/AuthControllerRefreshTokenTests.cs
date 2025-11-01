using System.Net;
using FluentAssertions;
using IdentityService.IntegrationTests.Base;
using IdentityService.IntegrationTests.Helpers;
using IdentityService.IntegrationTests.Infrastructure;
using IdentityService.Shared.DTOs.Response.Auth;
using IdentityService.Shared.Response;
using Microsoft.EntityFrameworkCore;
using Xunit;
using ZivraFramework.Core.Utils;

namespace IdentityService.IntegrationTests.Controllers;

public class AuthControllerRefreshTokenTests : BaseIntegrationTest
{
    public AuthControllerRefreshTokenTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Refresh_WithValidRefreshToken_ShouldReturnNewTokens()
    {
        // Arrange
        const string password = "TestPassword123!";
        var user = await CreateTestUserAsync("testuser", "test@example.com", password, emailConfirmed: true);
        var role = await CreateTestRoleAsync("TestRole");
        await AssignRoleToUserAsync(user.Id, role.Id);
        
        var loginRequest = TestDataGenerator.GenerateValidLoginRequest("testuser", password);
        var loginResponse = await PostAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.ShouldDeserializeTo<Result<AuthResponse>>();
        
        // Set refresh token cookie
        Client.DefaultRequestHeaders.Add("Cookie", $"access_token={loginResult.Data!.Token}");
        Client.DefaultRequestHeaders.Add("Cookie", $"refresh_token={loginResult.Data!.RefreshToken}");

        // Act
        var response = await Client.PostAsync($"/api/auth/refresh?userId={user.Id}", null);

        // Assert
        response.ShouldBeSuccessStatusCode();
        response.ShouldHaveCookie("access_token");
        response.ShouldHaveCookie("refresh_token");

        var result = await response.ShouldDeserializeTo<Result<AuthResponse>>();
        result.Should().NotBeNull();
        result.Data.Username.Should().Be("testuser");
        result.Data.Token.Should().NotBeNullOrEmpty();
        result.Data.Token.Should().NotBe(loginResult.Data.Token); // Should be a new token
        result.Data.RefreshToken.Should().NotBeNullOrEmpty();
        result.Data.RefreshToken.Should().NotBe(loginResult.Data.RefreshToken); // Should be a new refresh token
        result.Data.Roles.Should().Contain("TestRole");
    }

    [Fact]
    public async Task Refresh_WithoutRefreshToken_ShouldReturnUnauthorized()
    {
        // Act
        var response = await Client.PostAsync("/api/auth/refresh", null);

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Refresh token tidak ditemukan");
    }

    [Fact]
    public async Task Refresh_WithInvalidRefreshToken_ShouldReturnUnauthorized()
    {
        // Arrange
        const string password = "TestPassword123!";
        var user = await CreateTestUserAsync("testuser", "test@example.com", password, emailConfirmed: true);
        var role = await CreateTestRoleAsync("TestRole");
        await AssignRoleToUserAsync(user.Id, role.Id);
        
        Client.DefaultRequestHeaders.Add("Cookie", "refresh_token=invalid-refresh-token");

        // Act
        var response = await Client.PostAsync($"/api/auth/refresh?userId={user.Id}", null);

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithExpiredRefreshToken_ShouldReturnUnauthorized()
    {
        // Arrange
        const string password = "TestPassword123!";
        var user = await CreateTestUserAsync("testuser", "test@example.com", password, emailConfirmed: true);
        
        var loginRequest = TestDataGenerator.GenerateValidLoginRequest("testuser", password);
        var loginResponse = await PostAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.ShouldDeserializeTo<Result<AuthResponse>>();

        // Manually expire the refresh token in database
        var refreshTokenInDb = Context.RefreshTokens.First(rt => rt.UserId == user.Id);
        refreshTokenInDb.Expires = DateTime.UtcNow.AddDays(-1); // Set to yesterday
        Context.RefreshTokens.Update(refreshTokenInDb);
        await Context.SaveChangesAsync();

        Client.DefaultRequestHeaders.Add("Cookie", $"access_token={loginResult.Data!.Token}");
        Client.DefaultRequestHeaders.Add("Cookie", $"refresh_token={loginResult.Data!.RefreshToken}");

        // Act
        var response = await Client.PostAsync($"/api/auth/refresh?userId={user.Id}", null);

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithRevokedRefreshToken_ShouldReturnUnauthorized()
    {
        // Arrange
        const string password = "TestPassword123!";
        var user = await CreateTestUserAsync("testuser", "test@example.com", password, emailConfirmed: true);
        
        var loginRequest = TestDataGenerator.GenerateValidLoginRequest("testuser", password);
        var loginResponse = await PostAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.ShouldDeserializeTo<Result<AuthResponse>>();

        // Manually revoke the refresh token in database
        var refreshTokenInDb = Context.RefreshTokens.First(rt => rt.UserId == user.Id);
        refreshTokenInDb.Revoked = DateTime.UtcNow;
        Context.RefreshTokens.Update(refreshTokenInDb);
        await Context.SaveChangesAsync();

        Client.DefaultRequestHeaders.Add("Cookie", $"access_token={loginResult.Data!.Token}");
        Client.DefaultRequestHeaders.Add("Cookie", $"refresh_token={loginResult.Data!.RefreshToken}");

        // Act
        var response = await Client.PostAsync($"/api/auth/refresh?userId={user.Id}", null);

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_ShouldRevokeOldRefreshToken()
    {
        // Arrange
        const string password = "TestPassword123!";
        var user = await CreateTestUserAsync("testuser", "test@example.com", password, emailConfirmed: true);
        
        var loginRequest = TestDataGenerator.GenerateValidLoginRequest("testuser", password);
        var loginResponse = await PostAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.ShouldDeserializeTo<Result<AuthResponse>>();
        
        var oldRefreshTokenId = Context.RefreshTokens.First(rt => rt.UserId == user.Id).Id;
        
        Client.DefaultRequestHeaders.Add("Cookie", $"access_token={loginResult.Data!.Token}");
        Client.DefaultRequestHeaders.Add("Cookie", $"refresh_token={loginResult.Data!.RefreshToken}");

        // Act
        var response = await Client.PostAsync($"/api/auth/refresh?userId={user.Id}", null);

        // Assert
        response.ShouldBeSuccessStatusCode();

        Context.ChangeTracker.Clear();
        
        var oldRefreshToken = await Context.RefreshTokens
            .AsNoTracking() // pastikan tidak ambil cache
            .FirstAsync(rt => rt.Id == oldRefreshTokenId);
        oldRefreshToken.Revoked.Should().NotBeNull();

        // Verify new refresh token is created
        var newRefreshTokens = Context.RefreshTokens.Where(rt => rt.UserId == user.Id && rt.Revoked == null);
        newRefreshTokens.Should().HaveCount(1);
    }

    [Fact]
    public async Task Refresh_ShouldCreateNewAccessToken()
    {
        // Arrange
        const string password = "TestPassword123!";
        var user = await CreateTestUserAsync("testuser", "test@example.com", password, emailConfirmed: true);
        
        var loginRequest = TestDataGenerator.GenerateValidLoginRequest("testuser", password);
        var loginResponse = await PostAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.ShouldDeserializeTo<Result<AuthResponse>>();
        
        Client.DefaultRequestHeaders.Add("Cookie", $"access_token={loginResult.Data!.Token}");
        Client.DefaultRequestHeaders.Add("Cookie", $"refresh_token={loginResult.Data!.RefreshToken}");

        // Act
        var response = await Client.PostAsync($"/api/auth/refresh?userId={user.Id}", null);

        // Assert
        response.ShouldBeSuccessStatusCode();

        // Verify new access token is created in database
        var accessTokens = Context.AccessTokens.Where(at => at.UserId == user.Id);
        accessTokens.Should().HaveCount(2); // One from login, one from refresh
    }
}
