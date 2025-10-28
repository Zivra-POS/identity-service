using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using ZivraFramework.EFCore.Repositories;

namespace IdentityService.Infrastructure.Repositories;

public class UserRoleRepository : GenericRepository<UserRole>, IUserRoleRepository
{
    public UserRoleRepository(IdentityDbContext ctx) : base(ctx)
    {
    }
    
    public async Task<IEnumerable<UserRole>> GetRowsByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _set.Where(ur => ur.UserId == userId).ToListAsync(ct);
    }
}