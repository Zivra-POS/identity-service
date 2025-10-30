using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using ZivraFramework.Core.Models;
using ZivraFramework.EFCore.Repositories;

namespace IdentityService.Infrastructure.Repositories;

public class UserBranchRepository : GenericRepository<UserBranch>, IUserBranchRepository
{
    public UserBranchRepository(IdentityDbContext ctx) : base(ctx) { }

    #region GetPagedByStoreAsync
    public async Task<PagedResult<UserBranch>> GetPagedByStoreAsync(PagedQuery query, Guid storeId, CancellationToken ct = default)
    {
        IQueryable<UserBranch> q = _set.AsNoTracking()
            .Include(ub => ub.User)
            .Include(ub => ub.Branch)
            .Where(ub => ub.Branch!.StoreId == storeId);
        
        var keyword = query?.Keyword ?? string.Empty;
        var limit = query?.Limit ?? 10;
        var offset = query?.Offset ?? 0;

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var s = keyword.Trim().ToLower();
            q = q.Where(ub => 
                EF.Functions.ILike(ub.User!.Username, "%" + s + "%") || 
                EF.Functions.ILike(ub.User!.Email!, "%" + s + "%") ||
                EF.Functions.ILike(ub.Branch!.Name, "%" + s + "%") || 
                EF.Functions.ILike(ub.Branch!.Code!, "%" + s + "%"));
        }

        var total = await q.CountAsync(ct);

        var items = await q
            .OrderByDescending(ub => ub.CreDate)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(ct);

        return new PagedResult<UserBranch>
        {
            Total = total,
            Items = items
        };
    }
    #endregion

    #region GetByUserIdAsync
    public async Task<IEnumerable<UserBranch>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _set.AsNoTracking()
            .Include(ub => ub.Branch)
            .Where(ub => ub.UserId == userId)
            .OrderByDescending(ub => ub.IsPrimary)
            .ThenBy(ub => ub.Branch!.Name)
            .ToListAsync(ct);
    }
    #endregion

    #region GetByBranchIdAsync
    public async Task<IEnumerable<UserBranch>> GetByBranchIdAsync(Guid branchId, CancellationToken ct = default)
    {
        return await _set.AsNoTracking()
            .Include(ub => ub.User)
            .Where(ub => ub.BranchId == branchId)
            .OrderBy(ub => ub.User!.Username)
            .ToListAsync(ct);
    }
    #endregion

    #region GetByUserAndBranchAsync
    public async Task<UserBranch?> GetByUserAndBranchAsync(Guid userId, Guid branchId, CancellationToken ct = default)
    {
        return await _set.AsNoTracking()
            .Include(ub => ub.User)
            .Include(ub => ub.Branch)
            .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BranchId == branchId, ct);
    }
    #endregion

    #region GetRowsForLookupAsync
    public async Task<IEnumerable<UserBranch>> GetRowsForLookupAsync(Guid storeId, CancellationToken ct = default)
    {
        return await _set.AsNoTracking()
            .Include(ub => ub.User)
            .Include(ub => ub.Branch)
            .Where(ub => ub.Branch!.StoreId == storeId)
            .OrderBy(ub => ub.User!.Username)
            .ThenBy(ub => ub.Branch!.Name)
            .ToListAsync(ct);
    }
    #endregion
}
