using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
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
            Total = total,
            Items = items
        };
    }
    #endregion
}
