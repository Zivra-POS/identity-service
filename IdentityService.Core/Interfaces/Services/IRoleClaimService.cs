using IdentityService.Shared.DTOs.RoleClaim;
using IdentityService.Shared.Response;

namespace IdentityService.Core.Interfaces.Services;

public interface IRoleClaimService
{
    Task<Result<IEnumerable<RoleClaimResponse>>> GetByRoleIdAsync(Guid roleId);
    Task<Result<RoleClaimResponse>> GetByIdAsync(Guid id);
    Task<Result<RoleClaimResponse>> CreateAsync(RoleClaimRequest req);
    Task<Result<RoleClaimResponse>> UpdateAsync(RoleClaimRequest req);
    Task<Result<string>> DeleteAsync(Guid id);
}

