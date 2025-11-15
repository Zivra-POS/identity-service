using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using ZivraFramework.Core.Models;
using ZivraFramework.EFCore.Repositories;

namespace IdentityService.Infrastructure.Repositories;

public class StoreRepository : GenericRepository<Store>, IStoreRepository
{
    public StoreRepository(IdentityDbContext ctx) : base(ctx) { }

    #region GetPagedAsync
    public async Task<PagedResult<Store>> GetPagedAsync(PagedQuery? query, CancellationToken ct = default)
    {
        IQueryable<Store> q = _set.AsNoTracking();
        
        var keyword = query?.Keyword ?? string.Empty;
        var limit = query?.Limit ?? 10;
        var offset = query?.Offset ?? 0;

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var s = keyword.Trim().ToLower();
            q = q.Where(st => 
                EF.Functions.ILike(st.Name, "%" + s + "%") || 
                EF.Functions.ILike(st.Code!, "%" + s + "%"));
        }

        var total = await q.CountAsync(ct);

        var items = await q
            .OrderByDescending(st => st.CreDate)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(ct);

        return new PagedResult<Store>
        {
            TotalCount = total,
            Items = items
        };
    }
    #endregion

    #region GetByCodeAsync
    public async Task<Store?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _set.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Code == code, ct);
    }
    #endregion

    #region IsCodeExistsAsync
    public async Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = _set.AsNoTracking().Where(s => s.Code == code);
        
        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }
        
        return await query.AnyAsync(ct);
    }
    #endregion
    
    #region GetLastCodeAsync
    public async Task<string?> GetLastCodeAsync(CancellationToken ct = default)
    {
        return await _set.AsNoTracking()
            .OrderByDescending(st => st.Code)
            .Select(st => st.Code ?? string.Empty)
            .FirstOrDefaultAsync(cancellationToken: ct);

    }
    #endregion

    #region GetByHashedIdAsync
    public async Task<Store?> GetByHashedIdAsync(string hashedId, CancellationToken ct = default)
    {
        return await _set.AsNoTracking()
            .FirstOrDefaultAsync(s => s.HashedId == hashedId, ct);
    }
    #endregion
}
