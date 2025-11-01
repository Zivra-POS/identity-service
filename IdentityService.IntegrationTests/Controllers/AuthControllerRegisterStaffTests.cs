using System.Net;
using FluentAssertions;
using IdentityService.IntegrationTests.Base;
using IdentityService.IntegrationTests.Helpers;
using IdentityService.IntegrationTests.Infrastructure;
using IdentityService.Shared.DTOs.Response;
using IdentityService.Shared.DTOs.Response.Auth;
using IdentityService.Shared.Response;
using Xunit;

namespace IdentityService.IntegrationTests.Controllers;

public class AuthControllerRegisterStaffTests : BaseIntegrationTest
{
    public AuthControllerRegisterStaffTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task RegisterStaff_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var ownerUser = await CreateTestUserAsync("owner", "owner@example.com", emailConfirmed: true);
        var ownerRole = await CreateTestRoleAsync("OWNER");
        await AssignRoleToUserAsync(ownerUser.Id, ownerRole.Id);
        
        var staffRole = await CreateTestRoleAsync("Staff");
        var store = await CreateTestStoreAsync("Owner's Store");
        var registerStaffRequest = TestDataGenerator.GenerateValidRegisterStaffRequest(
            storeId: store.Id,
            ownerId: ownerUser.Id, 
            roleIds: new[] { staffRole.Id }
        );

        // Login as owner first
        var loginRequest = TestDataGenerator.GenerateValidLoginRequest("owner", "TestPassword123!");
        var loginResponse = await PostAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.ShouldDeserializeTo<Result<AuthResponse>>();
        SetBearerToken(loginResult.Data!.Token);

        var formData = MultipartFormDataHelper.CreateFormData(registerStaffRequest, 
            new Dictionary<string, byte[]> { { "ProfileImage", "test-image-content"u8.ToArray() } });

        // Act
        var response = await PostFormDataAsync("/api/auth/register-staff", formData);

        // Assert
        response.ShouldBeSuccessStatusCode();
        var result = await response.ShouldDeserializeTo<Result<RegisterStaffResponse>>();
        
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Username.Should().Be(registerStaffRequest.Username);
        result.Data.Email.Should().Be(registerStaffRequest.Email);

        // Verify user is created in database
        var userInDb = Context.Users.FirstOrDefault(u => u.Username == registerStaffRequest.Username);
        userInDb.Should().NotBeNull();
        userInDb!.OwnerId.Should().Be(ownerUser.Id);
        userInDb.StoreId.Should().Be(store.Id);
    }

    [Fact]
    public async Task RegisterStaff_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        var registerStaffRequest = TestDataGenerator.GenerateValidRegisterStaffRequest();
        var formData = MultipartFormDataHelper.CreateFormData(registerStaffRequest);

        // Act
        var response = await PostFormDataAsync("/api/auth/register-staff", formData);

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RegisterStaff_WithDuplicateUsername_ShouldReturnBadRequest()
    {
        // Arrange
        var ownerUser = await CreateTestUserAsync("owner", "owner@example.com", emailConfirmed: true);
        var existingStaff = await CreateTestUserAsync("existingstaff", "existing@example.com");
        
        var loginRequest = TestDataGenerator.GenerateValidLoginRequest("owner", "TestPassword123!");
        var loginResponse = await PostAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.ShouldDeserializeTo<Result<AuthResponse>>();
        SetBearerToken(loginResult.Data!.Token);

        var registerStaffRequest = TestDataGenerator.GenerateValidRegisterStaffRequest();
        registerStaffRequest.Username = existingStaff.Username;

        var formData = MultipartFormDataHelper.CreateFormData(registerStaffRequest);

        // Act
        var response = await PostFormDataAsync("/api/auth/register-staff", formData);

        // Debug: if not success, print status and response body to help diagnose 500 errors
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[TEST DEBUG] Status: {(int)response.StatusCode} {response.StatusCode}");
            Console.WriteLine($"[TEST DEBUG] Response Body: {errorContent}");
        }

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Username telah digunakan");
    }

    [Fact]
    public async Task RegisterStaff_WithDuplicateEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var ownerUser = await CreateTestUserAsync("owner", "owner@example.com", emailConfirmed: true);
        var existingStaff = await CreateTestUserAsync("existingstaff", "existing@example.com");
        
        var loginRequest = TestDataGenerator.GenerateValidLoginRequest("owner", "TestPassword123!");
        var loginResponse = await PostAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.ShouldDeserializeTo<Result<AuthResponse>>();
        SetBearerToken(loginResult.Data!.Token);

        var registerStaffRequest = TestDataGenerator.GenerateValidRegisterStaffRequest();
        registerStaffRequest.Email = existingStaff.Email;

        var formData = MultipartFormDataHelper.CreateFormData(registerStaffRequest);

        // Act
        var response = await PostFormDataAsync("/api/auth/register-staff", formData);

        // Debug: if not success, print status and response body to help diagnose 500 errors
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[TEST DEBUG] Status: {(int)response.StatusCode} {response.StatusCode}");
            Console.WriteLine($"[TEST DEBUG] Response Body: {errorContent}");
        }

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Email telah digunakan");
    }

    [Fact]
    public async Task RegisterStaff_ShouldAssignRoles()
    {
        // Arrange
        var ownerUser = await CreateTestUserAsync("owner", "owner@example.com", emailConfirmed: true);
        var role1 = await CreateTestRoleAsync("Manager");
        var role2 = await CreateTestRoleAsync("Cashier");
        
        var loginRequest = TestDataGenerator.GenerateValidLoginRequest("owner", "TestPassword123!");
        var loginResponse = await PostAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.ShouldDeserializeTo<Result<AuthResponse>>();
        SetBearerToken(loginResult.Data!.Token);

        var registerStaffRequest = TestDataGenerator.GenerateValidRegisterStaffRequest(
            roleIds: new[] { role1.Id, role2.Id }
        );

        var formData = MultipartFormDataHelper.CreateFormData(registerStaffRequest);

        // Act
        var response = await PostFormDataAsync("/api/auth/register-staff", formData);

        // Debug: if not success, print status and response body to help diagnose 500 errors
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[TEST DEBUG] Status: {(int)response.StatusCode} {response.StatusCode}");
            Console.WriteLine($"[TEST DEBUG] Response Body: {errorContent}");
        }

        // Assert
        response.ShouldBeSuccessStatusCode();

        var userInDb = Context.Users.FirstOrDefault(u => u.Username == registerStaffRequest.Username);
        userInDb.Should().NotBeNull();

        var userRoles = Context.UserRoles.Where(ur => ur.UserId == userInDb!.Id).ToList();
        userRoles.Should().HaveCount(2);
        userRoles.Should().Contain(ur => ur.RoleId == role1.Id);
        userRoles.Should().Contain(ur => ur.RoleId == role2.Id);
    }

    [Fact]
    public async Task RegisterStaff_ShouldCreateSecurityLog()
    {
        // Arrange
        var ownerUser = await CreateTestUserAsync("owner", "owner@example.com", emailConfirmed: true);
        
        var loginRequest = TestDataGenerator.GenerateValidLoginRequest("owner", "TestPassword123!");
        var loginResponse = await PostAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.ShouldDeserializeTo<Result<AuthResponse>>();
        SetBearerToken(loginResult.Data!.Token);

        var registerStaffRequest = TestDataGenerator.GenerateValidRegisterStaffRequest();
        var formData = MultipartFormDataHelper.CreateFormData(registerStaffRequest);

        // Act
        var response = await PostFormDataAsync("/api/auth/register-staff", formData);

        // Debug: if not success, print status and response body to help diagnose 500 errors
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[TEST DEBUG] Status: {(int)response.StatusCode} {response.StatusCode}");
            Console.WriteLine($"[TEST DEBUG] Response Body: {errorContent}");
        }

        // Assert
        response.ShouldBeSuccessStatusCode();

        var userInDb = Context.Users.FirstOrDefault(u => u.Username == registerStaffRequest.Username);
        userInDb.Should().NotBeNull();

        var securityLog = Context.SecurityLogs.FirstOrDefault(sl => sl.UserId == userInDb!.Id);
        securityLog.Should().NotBeNull();
    }

    [Fact]
    public async Task RegisterStaff_ShouldCreatePasswordHistory()
    {
        // Arrange
        var ownerUser = await CreateTestUserAsync("owner", "owner@example.com", emailConfirmed: true);
        
        var loginRequest = TestDataGenerator.GenerateValidLoginRequest("owner", "TestPassword123!");
        var loginResponse = await PostAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.ShouldDeserializeTo<Result<AuthResponse>>();
        SetBearerToken(loginResult.Data!.Token);

        var registerStaffRequest = TestDataGenerator.GenerateValidRegisterStaffRequest();
        var formData = MultipartFormDataHelper.CreateFormData(registerStaffRequest);

        // Act
        var response = await PostFormDataAsync("/api/auth/register-staff", formData);

        // Debug: if not success, print status and response body to help diagnose 500 errors
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[TEST DEBUG] Status: {(int)response.StatusCode} {response.StatusCode}");
            Console.WriteLine($"[TEST DEBUG] Response Body: {errorContent}");
        }

        // Assert
        response.ShouldBeSuccessStatusCode();

        var userInDb = Context.Users.FirstOrDefault(u => u.Username == registerStaffRequest.Username);
        userInDb.Should().NotBeNull();

        var passwordHistory = Context.PasswordHistories.FirstOrDefault(ph => ph.UserId == userInDb!.Id);
        passwordHistory.Should().NotBeNull();
    }
}
