using IdentityService.Shared.DTOs.Response.Role;

namespace IdentityService.Core.Mappers.Role;

public static class RoleMapper
{
    public static RoleResponse ToResponse(Entities.Role r)
    {
        return new RoleResponse
        {
            Id = r.Id,
            Name = r.Name,
            NormalizedName = r.NormalizedName,
            Description = r.Description,
            CreDate = r.CreDate,
            ModDate = r.ModDate
        };
    }
}
