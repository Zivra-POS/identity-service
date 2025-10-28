using IdentityService.Core.Entities;
using IdentityService.Shared.DTOs.UserClaim;

namespace IdentityService.Core.Mappers.User;

public static class UserClaimMapper
{
    public static UserClaimResponse ToResponse(UserClaim uc)
    {
        return new UserClaimResponse
        {
            Id = uc.Id,
            UserId = uc.UserId,
            ClaimType = uc.ClaimType,
            ClaimValue = uc.ClaimValue,
            CreDate = uc.CreDate,
            ModDate = uc.ModDate
        };
    }
}
