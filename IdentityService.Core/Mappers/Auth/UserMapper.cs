using System.Linq;
using IdentityService.Core.Entities;
using IdentityService.Shared.DTOs.Response.Auth;

namespace IdentityService.Core.Mappers;

public static class UserMapper
{
    public static UserResponse ToUserResponse(Entities.User user)
    {
        var roles = user.UserRoles
            .Select(ur => ur.Role?.Name)
            .Where(rn => !string.IsNullOrWhiteSpace(rn))
            .Select(rn => rn!)
            .ToArray();

        return new UserResponse
        {
            Id = user.Id,
            FullName = user.FullName ?? string.Empty,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            ProfileUrl = user.ProfileUrl,
            IsActive = user.IsActive,

            StoreId = user.StoreId,
            OwnerId = user.OwnerId,
            IsFirstLogin = user.IsFirstLogin,

            Address = user.Address,
            Province = user.Province,
            City = user.City,
            District = user.District,
            Rt = user.Rt,
            Rw = user.Rw,

            CreDate = user.CreDate,
            CreBy = user.CreBy,
            CreIpAddress = user.CreIpAddress,
            ModDate = user.ModDate,
            ModBy = user.ModBy,
            ModIpAddress = user.ModIpAddress,

            Roles = roles
        };
    }
}
