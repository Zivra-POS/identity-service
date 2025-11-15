using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using ZivraFramework.Core.Filtering;
using ZivraFramework.Core.Filtering.Entities;
using ZivraFramework.Core.Models;
using ZivraFramework.EFCore.Repositories;

namespace IdentityService.Infrastructure.Repositories;

public class BranchRepository : GenericRepository<Branch>, IBranchRepository
{
    public BranchRepository(IdentityDbContext ctx) : base(ctx) { }

    #region GetPagedByStoreAsync
    public async Task<PagedResult<Branch>> GetPagedByStoreAsync(PagedQuery query, Guid storeId, CancellationToken ct = default)
    {
        IQueryable<Branch> q = _set.AsNoTracking().Where(b => b.StoreId == storeId);
        
        var keyword = query?.Keyword ?? string.Empty;
        var limit = query?.Limit ?? 10;
        var offset = query?.Offset ?? 0;

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var s = keyword.Trim().ToLower();
            q = q.Where(b => 
                EF.Functions.ILike(b.Name, "%" + s + "%") || 
                EF.Functions.ILike(b.Code!, "%" + s + "%"));
        }

        var total = await q.CountAsync(ct);

        var items = await q
            .OrderByDescending(b => b.CreDate)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(ct);

        return new PagedResult<Branch>
        {
            TotalCount = total,
            Items = items
        };
    }
    #endregion
    
    #region GetAllByStoreIdAsync
    public async Task<PagedResult<Branch>> GetAllByStoreIdAsync(QueryRequest req, Guid storeId, CancellationToken ct = default)
    {
        var q = _set.AsNoTracking().ApplyFiltering(req);

        if (req?.Sorts == null || !req.Sorts.Any())
        {
            q = q.OrderByDescending(b => b.CreDate);
        }

        var count = await q.CountAsync(ct);

        var branches = await q
            .Where(b => b.StoreId == storeId)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Branch>()
        {
            Items = branches,
            TotalCount = count,
        };
    }
    #endregion

    #region GetByHashedIdAsync
    public async Task<Branch?> GetByHashedIdAsync(string hashedId, CancellationToken ct = default)
    {
        return await _set.FirstOrDefaultAsync(b => b.HashedId == hashedId, ct);
    }
    #endregion
}
