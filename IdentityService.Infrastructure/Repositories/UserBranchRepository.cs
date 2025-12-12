using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Shared.DTOs.Response.UserBranch;
using Microsoft.EntityFrameworkCore;
using ZivraFramework.Core.Filtering;
using ZivraFramework.Core.Filtering.Entities;
using ZivraFramework.Core.Models;
using ZivraFramework.EFCore.Repositories;

namespace IdentityService.Infrastructure.Repositories;

public class UserBranchRepository : GenericRepository<UserBranch>, IUserBranchRepository
{
    public UserBranchRepository(IdentityDbContext ctx) : base(ctx)
    {
    }

    #region GetPagedByStoreAsync
    public async Task<PagedResult<UserBranchResponse>> GetPagedByStoreAsync(QueryRequest query, Guid storeId,
        CancellationToken ct = default)
    {
        IQueryable<UserBranch> q = _set.AsNoTracking()
            .Include(ub => ub.User)
            .Include(ub => ub.Branch)
            .ApplyFiltering(query)
            .Where(ub => ub.Branch!.StoreId == storeId);

        var total = await q.CountAsync(ct);

        var items = await q
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(ub => new UserBranchResponse
            {
                HashedId = ub.HashedId,
                Id = ub.Id,
                UserId = ub.UserId,
                BranchId = ub.BranchId,
                IsPrimary = ub.IsPrimary,
                BranchName = ub.Branch!.Name,
                BranchCode = ub.Branch.Code,
            })
            .ToListAsync(ct);

        return new PagedResult<UserBranchResponse>
        {
            TotalCount = total,
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

    #region GetByHashedIdAsync

    public async Task<UserBranch?> GetByHashedIdAsync(string hashedId, CancellationToken ct = default)
    {
        return await _set.AsNoTracking()
            .Include(ub => ub.User)
            .Include(ub => ub.Branch)
            .FirstOrDefaultAsync(ub => ub.HashedId == hashedId, ct);
    }

    #endregion

    #region SetFalseIsPrimaryAsync
    public async Task SetFalseIsPrimaryAsync(Guid userId, CancellationToken ct = default)
    {
        await _set
            .Where(ub => ub.UserId == userId && ub.IsPrimary)
            .ExecuteUpdateAsync(updater => updater
                    .SetProperty(ub => ub.IsPrimary, false),
                ct
            );
    }
    #endregion
}
