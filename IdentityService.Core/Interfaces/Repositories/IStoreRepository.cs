using IdentityService.Core.Entities;
using ZivraFramework.Core.Interfaces;
using ZivraFramework.Core.Models;

namespace IdentityService.Core.Interfaces.Repositories;

public interface IStoreRepository : IGenericRepository<Store>
{
    Task<PagedResult<Store>> GetPagedAsync(PagedQuery query, CancellationToken ct = default);
    Task<Store?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default);
    Task<string?> GetLastCodeAsync(CancellationToken ct = default);
    Task<Store?> GetByHashedIdAsync(string hashedId, CancellationToken ct = default);
}
