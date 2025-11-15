using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using ZivraFramework.EFCore.Repositories;

namespace IdentityService.Infrastructure.Repositories;

public class RoleRepository : GenericRepository<Role>, IRoleRepository
{
    public RoleRepository(IdentityDbContext ctx) : base(ctx)
    {
    }

    #region GetByIdsAsync
    public async Task<IEnumerable<Role>> GetByIdsAsync(IEnumerable<Guid>? ids, CancellationToken ct = default)
    {
        if (ids == null) return Array.Empty<Role>();
        var idList = ids as IList<Guid> ?? ids.ToList();
        if (idList.Count == 0) return Array.Empty<Role>();

        return await _set
            .Where(r => idList.Contains(r.Id))
            .ToListAsync(ct);
    }
    #endregion

    #region GetByNameAsync
    public async Task<Role?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await _set
            .FirstOrDefaultAsync(r => r.NormalizedName == name, ct);
    }
    #endregion

    #region GetByHashedIdAsync
    public async Task<Role?> GetByHashedIdAsync(string hashedId, CancellationToken ct = default)
    {
        return await _set.FirstOrDefaultAsync(r => r.HashedId == hashedId, ct);
    }
    #endregion
}