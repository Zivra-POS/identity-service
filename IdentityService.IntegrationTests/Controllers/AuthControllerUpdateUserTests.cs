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

public class AuthControllerUpdateUserTests : BaseIntegrationTest
{
    public AuthControllerUpdateUserTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdateUser_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        const string password = "TestPassword123!";
        var user = await CreateTestUserAsync("testuser", "test@example.com", password, emailConfirmed: true);
        
        var loginRequest = TestDataGenerator.GenerateValidLoginRequest("testuser", password);
        var loginResponse = await PostAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.ShouldDeserializeTo<Result<AuthResponse>>();
        SetBearerToken(loginResult.Data!.Token);

        var updateUserRequest = TestDataGenerator.GenerateValidUpdateUserRequest(user.Id);
        var formData = MultipartFormDataHelper.CreateFormData(updateUserRequest,
            new Dictionary<string, byte[]> { { "ProfilImage", "updated-image-content"u8.ToArray() } });

        // Act
        var response = await PutFormDataAsync("/api/auth", formData);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[TEST DEBUG] Status: {(int)response.StatusCode} {response.StatusCode}");
            Console.WriteLine($"[TEST DEBUG] Response Body: {errorContent}");
        }

        // Assert
        response.ShouldBeSuccessStatusCode();
        var result = await response.ShouldDeserializeTo<Result<UpdateUserResponse>>();
        
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.DisplayName.Should().Be(updateUserRequest.DisplayName);
        result.Data.PhoneNumber.Should().Be(updateUserRequest.PhoneNumber);

        // Verify user is updated in database
        Context.ChangeTracker.Clear();
        var userInDb = await Context.Users.AsNoTracking().FirstAsync(u => u.Id == user.Id);
        userInDb.DisplayName.Should().Be(updateUserRequest.DisplayName);
        userInDb.PhoneNumber.Should().Be(updateUserRequest.PhoneNumber);
        userInDb.Address.Should().Be(updateUserRequest.Address);
        userInDb.ModDate.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateUser_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        var updateUserRequest = TestDataGenerator.GenerateValidUpdateUserRequest(Guid.NewGuid());
        var formData = MultipartFormDataHelper.CreateFormData(updateUserRequest);

        // Act
        var response = await PutFormDataAsync("/api/auth", formData);

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateUser_WithNonExistentUser_ShouldReturnNotFound()
    {
        // Arrange
        const string password = "TestPassword123!";
        var user = await CreateTestUserAsync("testuser", "test@example.com", password, emailConfirmed: true);
        
        var loginRequest = TestDataGenerator.GenerateValidLoginRequest("testuser", password);
        var loginResponse = await PostAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.ShouldDeserializeTo<Result<AuthResponse>>();
        SetBearerToken(loginResult.Data!.Token);

        var updateUserRequest = TestDataGenerator.GenerateValidUpdateUserRequest(Guid.NewGuid()); // Non-existent user
        var formData = MultipartFormDataHelper.CreateFormData(updateUserRequest);

        // Act
        var response = await PutFormDataAsync("/api/auth", formData);

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("User tidak ditemukan");
    }

    [Fact]
    public async Task UpdateUser_ShouldCreateSecurityLog()
    {
        // Arrange
        const string password = "TestPassword123!";
        var user = await CreateTestUserAsync("testuser", "test@example.com", password, emailConfirmed: true);
        
        var loginRequest = TestDataGenerator.GenerateValidLoginRequest("testuser", password);
        var loginResponse = await PostAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.ShouldDeserializeTo<Result<AuthResponse>>();
        SetBearerToken(loginResult.Data!.Token);

        var updateUserRequest = TestDataGenerator.GenerateValidUpdateUserRequest(user.Id);
        var formData = MultipartFormDataHelper.CreateFormData(updateUserRequest);

        // Act
        var response = await PutFormDataAsync("/api/auth", formData);

        // Assert
        response.ShouldBeSuccessStatusCode();
        
        Context.ChangeTracker.Clear();
        var securityLog = Context.SecurityLogs
            .AsNoTracking()
            .FirstOrDefault(sl => sl.UserId == user.Id && sl.Action == "Akun Diperbarui");
        securityLog.Should().NotBeNull();
        securityLog!.Description.Should().Contain("Akun diperbarui");
    }

    [Fact]
    public async Task UpdateUser_WithFileUpload_ShouldUpdateProfileImage()
    {
        // Arrange
        const string password = "TestPassword123!";
        var user = await CreateTestUserAsync("testuser", "test@example.com", password, emailConfirmed: true);
        
        var loginRequest = TestDataGenerator.GenerateValidLoginRequest("testuser", password);
        var loginResponse = await PostAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.ShouldDeserializeTo<Result<AuthResponse>>();
        SetBearerToken(loginResult.Data!.Token);

        var updateUserRequest = TestDataGenerator.GenerateValidUpdateUserRequest(user.Id);
        var formData = MultipartFormDataHelper.CreateFormData(updateUserRequest,
            new Dictionary<string, byte[]> { { "ProfilImage", "new-profile-image-content"u8.ToArray() } });

        // Act
        var response = await PutFormDataAsync("/api/auth", formData);

        // Assert
        response.ShouldBeSuccessStatusCode();

        Context.ChangeTracker.Clear();
        var userInDb = Context.Users.AsNoTracking().First(u => u.Id == user.Id);
        userInDb.ProfileUrl.Should().NotBeNullOrEmpty();
        userInDb.ProfileUrl.Should().NotBe(user.ProfileUrl); // Should be updated
    }

    [Fact]
    public async Task UpdateUser_WithPartialData_ShouldUpdateOnlyProvidedFields()
    {
        // Arrange
        const string password = "TestPassword123!";
        var user = await CreateTestUserAsync("testuser", "test@example.com", password, emailConfirmed: true);
        user.DisplayName = "Original Display Name";
        user.PhoneNumber = "Original Phone";
        Context.Users.Update(user);
        await Context.SaveChangesAsync();
        
        var loginRequest = TestDataGenerator.GenerateValidLoginRequest("testuser", password);
        var loginResponse = await PostAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.ShouldDeserializeTo<Result<AuthResponse>>();
        SetBearerToken(loginResult.Data!.Token);

        var updateUserRequest = TestDataGenerator.GenerateValidUpdateUserRequest(user.Id);
        updateUserRequest.DisplayName = "New Display Name";
        updateUserRequest.PhoneNumber = null; // Don't update phone number

        var formData = MultipartFormDataHelper.CreateFormData(updateUserRequest);

        // Act
        var response = await PutFormDataAsync("/api/auth", formData);

        // Assert
        response.ShouldBeSuccessStatusCode();
        
        Context.ChangeTracker.Clear();
        var userInDb = Context.Users.AsNoTracking().First(u => u.Id == user.Id);
        userInDb.DisplayName.Should().Be("New Display Name");
        userInDb.PhoneNumber.Should().BeNull(); // Should be updated to null
    }
}
