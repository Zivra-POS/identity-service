using IdentityService.Core.Entities;
using IdentityService.Shared.DTOs.RoleClaim;

namespace IdentityService.Core.Mappers.Role;

public static class RoleClaimMapper
{
    public static RoleClaimResponse ToResponse(RoleClaim rc)
    {
        return new RoleClaimResponse
        {
            Id = rc.Id,
            RoleId = rc.RoleId,
            ClaimType = rc.ClaimType,
            ClaimValue = rc.ClaimValue,
            CreDate = rc.CreDate,
            ModDate = rc.ModDate
        };
    }
}

