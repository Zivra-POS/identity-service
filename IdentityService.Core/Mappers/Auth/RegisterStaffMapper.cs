using IdentityService.Shared.DTOs.Response;
using IdentityService.Shared.DTOs.Response.Auth;

namespace IdentityService.Core.Mappers;

public class RegisterStaffMapper
{
    public static RegisterStaffResponse ToRegisterStaffResponse(Entities.User user)
    {
        return new RegisterStaffResponse
        {
            Id = user.Id,
            HashedId = user.HashedId,
            FullName = user.FullName ?? string.Empty,
            Username = user.Username,
            Email = user.Email,
            DisplayName = user.DisplayName,
            PhoneNumber = user.PhoneNumber,
            ProfileUrl = user.ProfileUrl,
            IsActive = user.IsActive,
            StoreId = user.StoreId,
            OwnerId = user.OwnerId,
            Address = user.Address,
            Province = user.Province,
            City = user.City,
            District = user.District,
            Rt = user.Rt,
            Rw = user.Rw,
            CreDate = user.CreDate,
            CreBy = user.CreBy,
            CreIpAddress = user.CreIpAddress,
            Roles = user.UserRoles
                .Select(ur => ur.Role!.Name)
                .ToArray()
        };
    }
}