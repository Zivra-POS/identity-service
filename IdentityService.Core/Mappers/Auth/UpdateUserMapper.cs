using IdentityService.Core.Entities;
using IdentityService.Shared.DTOs.Response;
using IdentityService.Shared.DTOs.Response.Auth;

namespace IdentityService.Core.Mappers;

public class UpdateUserMapper
{
    public static UpdateUserResponse ToUpdateUserResponse(Entities.User req)
    {
        return new UpdateUserResponse
        {
            Id = req.Id,
            FullName = req.FullName ?? string.Empty,
            Username = req.Username,
            DisplayName = req.DisplayName,
            Email = req.Email,
            PhoneNumber = req.PhoneNumber,
            ProfileUrl = req.ProfileUrl,
            IsActive = req.IsActive,

            // Address fields
            Address = req.Address,
            Province = req.Province,
            City = req.City,
            District = req.District,
            Rt = req.Rt,
            Rw = req.Rw,

            ModDate = req.ModDate,
            ModBy = req.ModBy,
            ModIpAddress = req.ModIpAddress,
        };
    }
}