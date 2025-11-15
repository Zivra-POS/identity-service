using IdentityService.Core.Entities;
using ZivraFramework.Core.Interfaces;

namespace IdentityService.Core.Interfaces.Repositories;

public interface IUserClaimRepository : IGenericRepository<UserClaim>
{
    Task<List<UserClaim>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    // Added: get by HashedId
    Task<UserClaim?> GetByHashedIdAsync(string hashedId, CancellationToken ct = default);
}
