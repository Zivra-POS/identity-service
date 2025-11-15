using IdentityService.Core.Entities;
using ZivraFramework.Core.Interfaces;
using ZivraFramework.Core.Models;

namespace IdentityService.Core.Interfaces.Repositories;

public interface IUserBranchRepository : IGenericRepository<UserBranch>
{
    Task<PagedResult<UserBranch>> GetPagedByStoreAsync(PagedQuery query, Guid storeId, CancellationToken ct = default);
    Task<IEnumerable<UserBranch>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<UserBranch>> GetByBranchIdAsync(Guid branchId, CancellationToken ct = default);
    Task<UserBranch?> GetByUserAndBranchAsync(Guid userId, Guid branchId, CancellationToken ct = default);
    Task<IEnumerable<UserBranch>> GetRowsForLookupAsync(Guid storeId, CancellationToken ct = default);
    Task<UserBranch?> GetByHashedIdAsync(string hashedId, CancellationToken ct = default);
}
