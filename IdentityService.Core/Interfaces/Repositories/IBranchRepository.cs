using IdentityService.Core.Entities;
using ZivraFramework.Core.Filtering.Entities;
using ZivraFramework.Core.Interfaces;
using ZivraFramework.Core.Models;

namespace IdentityService.Core.Interfaces.Repositories;

public interface IBranchRepository : IGenericRepository<Branch>
{
    Task<PagedResult<Branch>> GetPagedByStoreAsync(ZivraFramework.Core.Models.PagedQuery query, Guid storeId, CancellationToken ct = default);
    Task<PagedResult<Branch>> GetAllByStoreIdAsync(QueryRequest req, Guid storeId, CancellationToken ct = default);

    // Added: get by HashedId
    Task<Branch?> GetByHashedIdAsync(string hashedId, CancellationToken ct = default);
}
