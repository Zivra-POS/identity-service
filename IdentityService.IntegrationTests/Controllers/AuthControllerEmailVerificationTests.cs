using System.Net;
using FluentAssertions;
using IdentityService.IntegrationTests.Base;
using IdentityService.IntegrationTests.Helpers;
using IdentityService.IntegrationTests.Infrastructure;
using IdentityService.Shared.Response;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IdentityService.IntegrationTests.Controllers;

public class AuthControllerEmailVerificationTests : BaseIntegrationTest
{
    public AuthControllerEmailVerificationTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task SendVerifyEmail_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var user = await CreateTestUserAsync("testuser", "test@example.com", emailConfirmed: false);
        var sendVerifyEmailRequest = TestDataGenerator.GenerateValidSendVerifyEmailRequest(user.Id, "test@example.com");

        // Act
        var response = await PostAsync("/api/auth/send-verify-email", sendVerifyEmailRequest);

        // Assert
        response.ShouldBeSuccessStatusCode();
        var result = await response.ShouldDeserializeTo<Result<string>>();
        
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNullOrEmpty();

        // Verify email verification token is created in database
        var userToken = Context.UserTokens
            .FirstOrDefault(ut => ut.UserId == user.Id && ut.Name == "EmailVerification");
        userToken.Should().NotBeNull();
    }

    [Fact]
    public async Task SendVerifyEmail_WithNonExistentUser_ShouldReturnNotFound()
    {
        // Arrange
        var sendVerifyEmailRequest = TestDataGenerator.GenerateValidSendVerifyEmailRequest(Guid.NewGuid(), "test@example.com");

        // Act
        var response = await PostAsync("/api/auth/send-verify-email", sendVerifyEmailRequest);

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("User tidak ditemukan");
    }

    [Fact]
    public async Task SendVerifyEmail_ShouldReplaceExistingToken()
    {
        // Arrange
        var user = await CreateTestUserAsync("testuser", "test@example.com", emailConfirmed: false);
        
        // Send first verification email
        var firstRequest = TestDataGenerator.GenerateValidSendVerifyEmailRequest(user.Id, "test@example.com");
        await PostAsync("/api/auth/send-verify-email", firstRequest);

        // Send second verification email
        var secondRequest = TestDataGenerator.GenerateValidSendVerifyEmailRequest(user.Id, "test@example.com");

        // Act
        var response = await PostAsync("/api/auth/send-verify-email", secondRequest);

        // Assert
        response.ShouldBeSuccessStatusCode();

        // Verify only one email verification token exists
        var userTokens = Context.UserTokens
            .Where(ut => ut.UserId == user.Id && ut.Name == "EmailVerification")
            .ToList();
        userTokens.Should().HaveCount(1);
    }

    [Fact]
    public async Task SendVerifyEmail_ShouldCreateSecurityLog()
    {
        // Arrange
        var user = await CreateTestUserAsync("testuser", "test@example.com", emailConfirmed: false);
        var sendVerifyEmailRequest = TestDataGenerator.GenerateValidSendVerifyEmailRequest(user.Id, "test@example.com");

        // Act
        var response = await PostAsync("/api/auth/send-verify-email", sendVerifyEmailRequest);

        // Assert
        response.ShouldBeSuccessStatusCode();

        Context.ChangeTracker.Clear();
        var securityLog = Context.SecurityLogs
            .AsNoTracking()
            .FirstOrDefault(sl => sl.UserId == user.Id && sl.Action == "Permintaan Verifikasi Email");
        securityLog.Should().NotBeNull();
        securityLog!.Description.Should().Contain("Email verifikasi dikirim");
    }

    [Fact]
    public async Task VerifyEmail_WithValidToken_ShouldReturnSuccess()
    {
        // Arrange
        var user = await CreateTestUserAsync("testuser", "test@example.com", emailConfirmed: false);
        
        // Send verification email first to get token
        var sendVerifyEmailRequest = TestDataGenerator.GenerateValidSendVerifyEmailRequest(user.Id, "test@example.com");
        var sendResponse = await PostAsync("/api/auth/send-verify-email", sendVerifyEmailRequest);
        var sendResult = await sendResponse.ShouldDeserializeTo<Result<string>>();

        // Act
        var response = await Client.PostAsync($"/api/auth/verify-email?token={sendResult.Data}", null);

        // Assert
        response.ShouldBeSuccessStatusCode();
        var result = await response.ShouldDeserializeTo<Result<string>>();
        
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Contain("Email berhasil diverifikasi");

        // Verify user email is confirmed in database
        Context.ChangeTracker.Clear();
        var userInDb = Context.Users.AsNoTracking().First(u => u.Id == user.Id);
        userInDb.EmailConfirmed.Should().BeTrue();

        // Verify token is deleted
        var userToken = Context.UserTokens
            .AsNoTracking()
            .FirstOrDefault(ut => ut.UserId == user.Id && ut.Name == "EmailVerification");
        userToken.Should().BeNull();
    }

    [Fact]
    public async Task VerifyEmail_WithInvalidToken_ShouldReturnBadRequest()
    {
        // Act
        var response = await Client.PostAsync("/api/auth/verify-email?token=invalid-token", null);

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Token verifikasi tidak valid");
    }

    [Fact]
    public async Task VerifyEmail_WithExpiredToken_ShouldReturnBadRequest()
    {
        // Arrange
        var user = await CreateTestUserAsync("testuser", "test@example.com", emailConfirmed: false);
        
        // Send verification email first
        var sendVerifyEmailRequest = TestDataGenerator.GenerateValidSendVerifyEmailRequest(user.Id, "test@example.com");
        var sendResponse = await PostAsync("/api/auth/send-verify-email", sendVerifyEmailRequest);
        var sendResult = await sendResponse.ShouldDeserializeTo<Result<string>>();

        // Manually expire the token in database
        var userToken = Context.UserTokens.First(ut => ut.UserId == user.Id && ut.Name == "EmailVerification");
        userToken.CreDate = DateTime.UtcNow.AddHours(-25); // Set to 25 hours ago (expired)
        Context.UserTokens.Update(userToken);
        await Context.SaveChangesAsync();

        // Act
        var response = await Client.PostAsync($"/api/auth/verify-email?token={sendResult.Data}", null);

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Token verifikasi telah kedaluwarsa");
    }

    [Fact]
    public async Task VerifyEmail_WithAlreadyVerifiedUser_ShouldStillWork()
    {
        // Arrange
        var user = await CreateTestUserAsync("testuser", "test@example.com", emailConfirmed: true);
        
        // Send verification email
        var sendVerifyEmailRequest = TestDataGenerator.GenerateValidSendVerifyEmailRequest(user.Id, "test@example.com");
        var sendResponse = await PostAsync("/api/auth/send-verify-email", sendVerifyEmailRequest);
        var sendResult = await sendResponse.ShouldDeserializeTo<Result<string>>();

        // Act
        var response = await Client.PostAsync($"/api/auth/verify-email?token={sendResult.Data}", null);

        // Assert
        response.ShouldBeSuccessStatusCode();
        
        // Verify user is still confirmed
        var userInDb = Context.Users.First(u => u.Id == user.Id);
        userInDb.EmailConfirmed.Should().BeTrue();
    }
}
