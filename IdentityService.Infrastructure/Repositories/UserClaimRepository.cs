using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using ZivraFramework.EFCore.Repositories;

namespace IdentityService.Infrastructure.Repositories;

public class UserClaimRepository : GenericRepository<UserClaim>, IUserClaimRepository
{
    public UserClaimRepository(IdentityDbContext ctx) : base(ctx)
    {
    }

    #region GetByUserIdAsync
    public async Task<List<UserClaim>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _set
            .Where(u => u.UserId == userId)
            .ToListAsync(ct);
    }
    #endregion
}