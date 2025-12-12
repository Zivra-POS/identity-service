using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Shared.DTOs.Response.Role;
using Microsoft.EntityFrameworkCore;
using ZivraFramework.Core.Filtering;
using ZivraFramework.Core.Filtering.Entities;
using ZivraFramework.Core.Models;
using ZivraFramework.EFCore.Repositories;

namespace IdentityService.Infrastructure.Repositories;

public class RoleRepository : GenericRepository<Role>, IRoleRepository
{
    public RoleRepository(IdentityDbContext ctx) : base(ctx)
    {
    }
    
    #region GetAllAsync
    public async Task<PagedResult<RoleResponse>> GetAllAsync(QueryRequest query, CancellationToken ct = default)
    {
        IQueryable<Role> q = _set
            .AsNoTracking()
            .ApplyFiltering(query);
        
        var total = await q.CountAsync(ct);
        
        var items = await q
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(r => new RoleResponse
            {
                Id = r.Id,
                HashedId = r.HashedId,
                Name = r.Name,
                NormalizedName = r.NormalizedName,
                Description = r.Description,
            })
            .ToListAsync(ct);
        
        return new PagedResult<RoleResponse>
        {
            TotalCount = total,
            Items = items
        };
    }
    #endregion

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