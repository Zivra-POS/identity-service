using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using ZivraFramework.EFCore.Repositories;
using IdentityDbContext = IdentityService.Infrastructure.Persistence.IdentityDbContext;

namespace IdentityService.Infrastructure.Repositories;

public class PasswordHistoryRepository : GenericRepository<PasswordHistory>, IPasswordHistoryRepository
{
    public PasswordHistoryRepository(IdentityDbContext ctx) : base(ctx)
    {
    }
    
    #region GetRowByPasswordHashAsync
    public async Task<PasswordHistory?> GetRowByPasswordHashAsync(string passwordHash, CancellationToken cancellationToken = default)
    {
        return await _set.FirstOrDefaultAsync(ph => ph.PasswordHash == passwordHash, cancellationToken);
    }
    #endregion
    
    #region GetRowsByUserIdAsync
    public async Task<List<PasswordHistory>> GetRowsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _set.Where(ph => ph.UserId == userId).ToListAsync(cancellationToken);
    }
    #endregion
}
