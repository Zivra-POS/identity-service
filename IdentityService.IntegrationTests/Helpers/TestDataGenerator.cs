using Bogus;
using IdentityService.Shared.DTOs.Request.Auth;
using IdentityService.Shared.DTOs.Request.User;
using Microsoft.AspNetCore.Http;

namespace IdentityService.IntegrationTests.Helpers;

public static class TestDataGenerator
{
    private static readonly Faker Faker = new("id_ID");

    public static RegisterRequest GenerateValidRegisterRequest()
    {
        return new RegisterRequest
        {
            Id = Guid.NewGuid(),
            FullName = Faker.Name.FullName(),
            Username = Faker.Internet.UserName(),
            Email = Faker.Internet.Email(),
            Password = "TestPassword123!",
            Address = Faker.Address.FullAddress(),
            Province = Faker.Address.State(),
            City = Faker.Address.City(),
            District = Faker.Address.County(),
            Rt = Faker.Random.Number(1, 20).ToString("00"),
            Rw = Faker.Random.Number(1, 15).ToString("00"),
            CreDate = DateTime.UtcNow,
            CreBy = "SYSTEM",
            CreIpAddress = Faker.Internet.Ip()
        };
    }

    public static RegisterStaffRequest GenerateValidRegisterStaffRequest(Guid? storeId = null, Guid? ownerId = null, Guid[]? roleIds = null)
    {
        return new RegisterStaffRequest
        {
            FullName = Faker.Name.FullName(),
            Username = Faker.Internet.UserName(),
            Email = Faker.Internet.Email(),
            Password = "TestPassword123!",
            DisplayName = Faker.Name.FirstName(),
            PhoneNumber = Faker.Phone.PhoneNumber(),
            IsActive = true,
            ProfileImage = CreateTestFormFile("test-profile.jpg", "image/jpeg"),
            StoreId = storeId,
            OwnerId = ownerId,
            RoleIDs = roleIds ?? new[] { Guid.NewGuid() },
            Address = Faker.Address.FullAddress(),
            Province = Faker.Address.State(),
            City = Faker.Address.City(),
            District = Faker.Address.County(),
            Rt = Faker.Random.Number(1, 20).ToString("00"),
            Rw = Faker.Random.Number(1, 15).ToString("00"),
            CreDate = DateTime.UtcNow,
            CreBy = "SYSTEM",
            CreIpAddress = Faker.Internet.Ip()
        };
    }

    public static UpdateUserRequest GenerateValidUpdateUserRequest(Guid userId)
    {
        return new UpdateUserRequest
        {
            Id = userId,
            FullName = Faker.Name.FullName(),
            DisplayName = Faker.Name.FirstName(),
            PhoneNumber = Faker.Phone.PhoneNumber(),
            IsActive = true,
            ProfilImage = CreateTestFormFile("updated-profile.jpg", "image/jpeg"),
            Address = Faker.Address.FullAddress(),
            Province = Faker.Address.State(),
            City = Faker.Address.City(),
            District = Faker.Address.County(),
            Rt = Faker.Random.Number(1, 20).ToString("00"),
            Rw = Faker.Random.Number(1, 15).ToString("00"),
            ModDate = DateTime.UtcNow,
            ModBy = "SYSTEM",
            ModIpAddress = Faker.Internet.Ip()
        };
    }

    public static LoginRequest GenerateValidLoginRequest(string username = "testuser", string password = "TestPassword123!")
    {
        return new LoginRequest
        {
            Username = username,
            Password = password,
            ModDate = DateTime.UtcNow,
            ModBy = "SYSTEM",
            ModIpAddress = Faker.Internet.Ip()
        };
    }

    public static ForgotPasswordRequest GenerateValidForgotPasswordRequest(string email = "test@example.com")
    {
        return new ForgotPasswordRequest
        {
            Id = Guid.NewGuid(),
            Email = email,
            CreDate = DateTime.UtcNow,
            CreBy = "SYSTEM",
            CreIpAddress = Faker.Internet.Ip(),
            ModDate = DateTime.UtcNow,
            ModBy = "SYSTEM",
            ModIpAddress = Faker.Internet.Ip()
        };
    }

    public static ResetPasswordRequest GenerateValidResetPasswordRequest(string token)
    {
        return new ResetPasswordRequest
        {
            Token = token,
            NewPassword = "NewPassword123!",
            ModDate = DateTime.UtcNow,
            ModBy = "SYSTEM",
            ModIpAddress = Faker.Internet.Ip()
        };
    }

    public static SendVerifyEmailRequest GenerateValidSendVerifyEmailRequest(Guid userId, string email = "test@example.com")
    {
        return new SendVerifyEmailRequest
        {
            UserId = userId,
            FullName = Faker.Name.FullName(),
            Email = email,
            Username = Faker.Internet.UserName(),
            IsSend = true,
            CreDate = DateTime.UtcNow,
            CreBy = "SYSTEM",
            CreIpAddress = Faker.Internet.Ip()
        };
    }

    public static UnlockUserRequest GenerateValidUnlockUserRequest(Guid userId)
    {
        return new UnlockUserRequest
        {
            UserId = userId,
            Reason = "Unlocked for testing purposes",
            ModDate = DateTime.UtcNow,
            ModBy = "ADMIN",
            ModIpAddress = Faker.Internet.Ip()
        };
    }

    private static IFormFile CreateTestFormFile(string fileName, string contentType)
    {
        var content = "Test file content"u8.ToArray();
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}
