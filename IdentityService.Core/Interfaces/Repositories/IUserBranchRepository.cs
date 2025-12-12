using IdentityService.Core.Entities;
using IdentityService.Shared.DTOs.Response.UserBranch;
using ZivraFramework.Core.Filtering.Entities;
using ZivraFramework.Core.Interfaces;
using ZivraFramework.Core.Models;

namespace IdentityService.Core.Interfaces.Repositories;

public interface IUserBranchRepository : IGenericRepository<UserBranch>
{
    Task<PagedResult<UserBranchResponse>> GetPagedByStoreAsync(QueryRequest query, Guid storeId, CancellationToken ct = default);
    Task<IEnumerable<UserBranch>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<UserBranch>> GetByBranchIdAsync(Guid branchId, CancellationToken ct = default);
    Task<UserBranch?> GetByUserAndBranchAsync(Guid userId, Guid branchId, CancellationToken ct = default);
    Task<IEnumerable<UserBranch>> GetRowsForLookupAsync(Guid storeId, CancellationToken ct = default);
    Task<UserBranch?> GetByHashedIdAsync(string hashedId, CancellationToken ct = default);
    Task SetFalseIsPrimaryAsync(Guid userId, CancellationToken ct = default);
}
