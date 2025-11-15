using IdentityService.Shared.DTOs.Request.Role;
using IdentityService.Shared.DTOs.RoleClaim;
using IdentityService.Shared.Response;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace IdentityService.Core.Interfaces.Services;

public interface IRoleClaimService
{
    Task<IEnumerable<RoleClaimResponse>> GetByRoleIdAsync(Guid roleId);
    Task<RoleClaimResponse> GetByIdAsync(Guid id);
    Task<RoleClaimResponse> CreateAsync(RoleClaimRequest req);
    Task<RoleClaimResponse> UpdateAsync(RoleClaimRequest req);
    Task<string> DeleteAsync(Guid id);

    // Added: get by hashed id
    Task<RoleClaimResponse> GetByHashedIdAsync(string hashedId);
}
