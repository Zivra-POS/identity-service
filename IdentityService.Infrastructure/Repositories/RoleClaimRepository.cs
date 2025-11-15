using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using ZivraFramework.EFCore.Repositories;

namespace IdentityService.Infrastructure.Repositories;

public class RoleClaimRepository : GenericRepository<RoleClaim>, IRoleClaimRepository
{
    public RoleClaimRepository(IdentityDbContext ctx) : base(ctx)
    {
    }

    #region GetByRoleIdAsync
    public async Task<List<RoleClaim>> GetByRoleIdAsync(Guid roleId, CancellationToken ct = default)
    {
        return await _set
            .Where(r => r.RoleId == roleId)
            .ToListAsync(ct);
    }
    #endregion

    #region GetByHashedIdAsync
    public async Task<RoleClaim?> GetByHashedIdAsync(string hashedId, CancellationToken ct = default)
    {
        return await _set.AsNoTracking()
            .FirstOrDefaultAsync(rc => rc.HashedId == hashedId, ct);
    }
    #endregion
}