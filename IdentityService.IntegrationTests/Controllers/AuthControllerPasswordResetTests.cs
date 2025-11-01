using System.Net;
using FluentAssertions;
using IdentityService.IntegrationTests.Base;
using IdentityService.IntegrationTests.Helpers;
using IdentityService.IntegrationTests.Infrastructure;
using IdentityService.Shared.DTOs.Response;
using IdentityService.Shared.DTOs.Response.Auth;
using IdentityService.Shared.Response;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IdentityService.IntegrationTests.Controllers;

public class AuthControllerPasswordResetTests : BaseIntegrationTest
{
    public AuthControllerPasswordResetTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ForgotPassword_WithValidEmail_ShouldReturnSuccess()
    {
        var user = await CreateTestUserAsync("testuser", "test@example.com", emailConfirmed: true);
        var forgotPasswordRequest = TestDataGenerator.GenerateValidForgotPasswordRequest("test@example.com");

        var response = await PostAsync("/api/auth/forgot", forgotPasswordRequest);

        response.ShouldBeSuccessStatusCode();
        var result = await response.ShouldDeserializeTo<Result<ForgotPasswordResponse>>();
        
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Token.Should().NotBeNullOrEmpty();

        var userToken = Context.UserTokens.FirstOrDefault(ut => ut.UserId == user.Id && ut.Name == "PasswordReset");
        userToken.Should().NotBeNull();
    }

    [Fact]
    public async Task ForgotPassword_WithNonExistentEmail_ShouldReturnNotFound()
    {
        var forgotPasswordRequest = TestDataGenerator.GenerateValidForgotPasswordRequest("nonexistent@example.com");

        var response = await PostAsync("/api/auth/forgot", forgotPasswordRequest);

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Email tidak terdaftar");
    }

    [Fact]
    public async Task ForgotPassword_ShouldCreateSecurityLog()
    {
        var user = await CreateTestUserAsync("testuser", "test@example.com", emailConfirmed: true);
        var forgotPasswordRequest = TestDataGenerator.GenerateValidForgotPasswordRequest("test@example.com");

        var response = await PostAsync("/api/auth/forgot", forgotPasswordRequest);

        response.ShouldBeSuccessStatusCode();
        
        Context.ChangeTracker.Clear();
        var securityLog = Context.SecurityLogs
            .AsNoTracking()
            .FirstOrDefault(sl => sl.UserId == user.Id && sl.Action == "Permintaan Reset Password");
        securityLog.Should().NotBeNull();
        securityLog!.Description.Should().Contain("Permintaan reset password dibuat");
    }

    [Fact]
    public async Task ResetPassword_WithValidToken_ShouldReturnSuccess()
    {
        var user = await CreateTestUserAsync("testuser", "test@example.com", emailConfirmed: true);
        
        var forgotPasswordRequest = TestDataGenerator.GenerateValidForgotPasswordRequest("test@example.com");
        var forgotResponse = await PostAsync("/api/auth/forgot", forgotPasswordRequest);
        var forgotResult = await forgotResponse.ShouldDeserializeTo<Result<ForgotPasswordResponse>>();

        var resetPasswordRequest = TestDataGenerator.GenerateValidResetPasswordRequest(forgotResult.Data!.Token);

        var response = await PostAsync("/api/auth/reset", resetPasswordRequest);

        response.ShouldBeSuccessStatusCode();
        var result = await response.ShouldDeserializeTo<Result<string>>();
        
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Contain("Password berhasil direset");

        Context.ChangeTracker.Clear();
        var userInDb = Context.Users.AsNoTracking().First(u => u.Id == user.Id);
        var isNewPasswordValid = BCrypt.Net.BCrypt.Verify("NewPassword123!", userInDb.PasswordHash);
        isNewPasswordValid.Should().BeTrue();

        var userToken = Context.UserTokens.FirstOrDefault(ut => ut.UserId == user.Id && ut.Name == "PasswordReset");
        userToken.Should().BeNull();
    }

    [Fact]
    public async Task ResetPassword_WithInvalidToken_ShouldReturnBadRequest()
    {
        var resetPasswordRequest = TestDataGenerator.GenerateValidResetPasswordRequest("invalid-token");

        var response = await PostAsync("/api/auth/reset", resetPasswordRequest);

        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Token reset tidak valid");
    }

    [Fact]
    public async Task ResetPassword_WithExpiredToken_ShouldReturnBadRequest()
    {
        var user = await CreateTestUserAsync("testuser", "test@example.com", emailConfirmed: true);
        
        var forgotPasswordRequest = TestDataGenerator.GenerateValidForgotPasswordRequest("test@example.com");
        var forgotResponse = await PostAsync("/api/auth/forgot", forgotPasswordRequest);
        var forgotResult = await forgotResponse.ShouldDeserializeTo<Result<ForgotPasswordResponse>>();

        var userToken = Context.UserTokens.First(ut => ut.UserId == user.Id && ut.Name == "PasswordReset");
        userToken.CreDate = DateTime.UtcNow.AddHours(-2);
        Context.UserTokens.Update(userToken);
        await Context.SaveChangesAsync();

        var resetPasswordRequest = TestDataGenerator.GenerateValidResetPasswordRequest(forgotResult.Data!.Token);

        var response = await PostAsync("/api/auth/reset", resetPasswordRequest);

        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Token reset telah kedaluwarsa");
    }

    [Fact]
    public async Task ResetPassword_WithPreviouslyUsedPassword_ShouldReturnBadRequest()
    {
        var registerRequest = TestDataGenerator.GenerateValidRegisterRequest();
        registerRequest.Username = "testuser";
        registerRequest.Email = "test@example.com";
        registerRequest.Password = "TestPassword123!";

        var registerResponse = await PostAsync("/api/auth/register", registerRequest);
        registerResponse.ShouldBeSuccessStatusCode();
        var registerResult = await registerResponse.ShouldDeserializeTo<Result<AuthResponse>>();

        var forgotPasswordRequest = TestDataGenerator.GenerateValidForgotPasswordRequest("test@example.com");
        var forgotResponse = await PostAsync("/api/auth/forgot", forgotPasswordRequest);
        var forgotResult = await forgotResponse.ShouldDeserializeTo<Result<ForgotPasswordResponse>>();

        var resetPasswordRequest = TestDataGenerator.GenerateValidResetPasswordRequest(forgotResult.Data!.Token);
        resetPasswordRequest.NewPassword = "TestPassword123!";

        var response = await PostAsync("/api/auth/reset", resetPasswordRequest);

        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Password baru tidak boleh sama dengan password sebelumnya.");
    }

    [Fact]
    public async Task ResetPassword_ShouldRevokeAllRefreshTokens()
    {
        var user = await CreateTestUserAsync("testuser", "test@example.com", emailConfirmed: true);
        
        var loginRequest = TestDataGenerator.GenerateValidLoginRequest("testuser", "TestPassword123!");
        await PostAsync("/api/auth/login", loginRequest);
        await PostAsync("/api/auth/login", loginRequest);

        var forgotPasswordRequest = TestDataGenerator.GenerateValidForgotPasswordRequest("test@example.com");
        var forgotResponse = await PostAsync("/api/auth/forgot", forgotPasswordRequest);
        var forgotResult = await forgotResponse.ShouldDeserializeTo<Result<ForgotPasswordResponse>>();

        var resetPasswordRequest = TestDataGenerator.GenerateValidResetPasswordRequest(forgotResult.Data!.Token);

        var response = await PostAsync("/api/auth/reset", resetPasswordRequest);

        response.ShouldBeSuccessStatusCode();

        var activeTokens = Context.RefreshTokens
            .Where(rt => rt.UserId == user.Id && rt.Revoked == null)
            .ToList();
        activeTokens.Should().BeEmpty();
    }

    [Fact]
    public async Task ResetPassword_ShouldCreateSecurityLog()
    {
        var user = await CreateTestUserAsync("testuser", "test@example.com", emailConfirmed: true);
        
        var forgotPasswordRequest = TestDataGenerator.GenerateValidForgotPasswordRequest("test@example.com");
        var forgotResponse = await PostAsync("/api/auth/forgot", forgotPasswordRequest);
        var forgotResult = await forgotResponse.ShouldDeserializeTo<Result<ForgotPasswordResponse>>();

        var resetPasswordRequest = TestDataGenerator.GenerateValidResetPasswordRequest(forgotResult.Data!.Token);

        var response = await PostAsync("/api/auth/reset", resetPasswordRequest);

        response.ShouldBeSuccessStatusCode();
    
        Context.ChangeTracker.Clear();
        var securityLog = Context.SecurityLogs
            .AsNoTracking()
            .FirstOrDefault(sl => sl.UserId == user.Id && sl.Action == "Password Diubah");
        securityLog.Should().NotBeNull();
        securityLog!.Description.Should().Contain("Password berhasil direset");
    }
}
