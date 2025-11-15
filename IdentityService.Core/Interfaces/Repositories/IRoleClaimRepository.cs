using IdentityService.Core.Entities;
using ZivraFramework.Core.Interfaces;

namespace IdentityService.Core.Interfaces.Repositories;

public interface IRoleClaimRepository : IGenericRepository<RoleClaim>
{
    Task<List<RoleClaim>> GetByRoleIdAsync(Guid claimId, CancellationToken ct = default);
    Task<RoleClaim?> GetByHashedIdAsync(string hashedId, CancellationToken ct = default);
}
