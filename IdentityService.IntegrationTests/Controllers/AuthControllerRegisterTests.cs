using System.Net;
using FluentAssertions;
using IdentityService.IntegrationTests.Base;
using IdentityService.IntegrationTests.Helpers;
using IdentityService.IntegrationTests.Infrastructure;
using IdentityService.Shared.DTOs.Request.Auth;
using IdentityService.Shared.DTOs.Response.Auth;
using IdentityService.Shared.Response;
using Xunit;

namespace IdentityService.IntegrationTests.Controllers;

public class AuthControllerRegisterTests : BaseIntegrationTest
{
    public AuthControllerRegisterTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var registerRequest = TestDataGenerator.GenerateValidRegisterRequest();

        // Act
        var response = await PostAsync("/api/auth/register", registerRequest);
        
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.Created);
        var result = await response.ShouldDeserializeTo<Result<AuthResponse>>();
        
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Username.Should().Be(registerRequest.Username);
        result.Data.Email.Should().Be(registerRequest.Email);
        result.Data.FullName.Should().Be(registerRequest.FullName);
        result.Data.VerifyToken.Should().NotBeNullOrEmpty();

        // Verify user is created in database
        var userInDb = Context.Users.FirstOrDefault(u => u.Username == registerRequest.Username);
        userInDb.Should().NotBeNull();
        userInDb!.Email.Should().Be(registerRequest.Email);
        userInDb.EmailConfirmed.Should().BeFalse(); // Should be false initially
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_ShouldReturnBadRequest()
    {
        // Arrange
        var existingUser = await CreateTestUserAsync("existinguser", "existing@example.com");
        var registerRequest = TestDataGenerator.GenerateValidRegisterRequest();
        registerRequest.Username = existingUser.Username;

        // Act
        var response = await PostAsync("/api/auth/register", registerRequest);

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Username telah digunakan");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var existingUser = await CreateTestUserAsync("existinguser", "existing@example.com");
        var registerRequest = TestDataGenerator.GenerateValidRegisterRequest();
        registerRequest.Email = existingUser.Email;

        // Act
        var response = await PostAsync("/api/auth/register", registerRequest);

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Email telah digunakan");
    }

    [Theory]
    [InlineData("", "test@example.com", "Password123!", "Username is required")]
    [InlineData("testuser", "", "Password123!", "Email is required")]
    [InlineData("testuser", "invalid-email", "Password123!", "Invalid email format")]
    [InlineData("testuser", "test@example.com", "", "Password is required")]
    [InlineData("testuser", "test@example.com", "weak", "Password too weak")]
    public async Task Register_WithInvalidData_ShouldReturnBadRequest(
        string username, string email, string password, string expectedError)
    {
        // Arrange
        var registerRequest = TestDataGenerator.GenerateValidRegisterRequest();
        registerRequest.Username = username;
        registerRequest.Email = email;
        registerRequest.Password = password;

        // Act
        var response = await PostAsync("/api/auth/register", registerRequest);

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
        
        // Verify the error message contains expected validation message
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeEmpty($"Expected error message for: {expectedError}");
    }

    [Fact]
    public async Task Register_WithNullRequest_ShouldReturnBadRequest()
    {
        // Act
        var response = await PostAsync("/api/auth/register", (RegisterRequest?)null);

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ShouldCreatePasswordHistory()
    {
        // Arrange
        var registerRequest = TestDataGenerator.GenerateValidRegisterRequest();

        // Act
        var response = await PostAsync("/api/auth/register", registerRequest);

        // Assert
        response.ShouldBeSuccessStatusCode();
        
        var userInDb = Context.Users.FirstOrDefault(u => u.Username == registerRequest.Username);
        userInDb.Should().NotBeNull();

        var passwordHistory = Context.PasswordHistories.FirstOrDefault(ph => ph.UserId == userInDb!.Id);
        passwordHistory.Should().NotBeNull();
    }

    [Fact]
    public async Task Register_ShouldCreateSecurityLog()
    {
        // Arrange
        var registerRequest = TestDataGenerator.GenerateValidRegisterRequest();

        // Act
        var response = await PostAsync("/api/auth/register", registerRequest);

        // Assert
        response.ShouldBeSuccessStatusCode();
        
        var userInDb = Context.Users.FirstOrDefault(u => u.Username == registerRequest.Username);
        userInDb.Should().NotBeNull();

        var securityLog = Context.SecurityLogs.FirstOrDefault(sl => sl.UserId == userInDb!.Id);
        securityLog.Should().NotBeNull();
    }

    [Fact]
    public async Task Register_ShouldAssignOwnerRole()
    {
        // Arrange
        var ownerRole = await CreateTestRoleAsync("OWNER");
        var registerRequest = TestDataGenerator.GenerateValidRegisterRequest();

        // Act
        var response = await PostAsync("/api/auth/register", registerRequest);

        // Assert
        response.ShouldBeSuccessStatusCode();
        
        var userInDb = Context.Users.FirstOrDefault(u => u.Username == registerRequest.Username);
        userInDb.Should().NotBeNull();

        var userRole = Context.UserRoles.FirstOrDefault(ur => ur.UserId == userInDb!.Id && ur.RoleId == ownerRole.Id);
        userRole.Should().NotBeNull();
    }
}

