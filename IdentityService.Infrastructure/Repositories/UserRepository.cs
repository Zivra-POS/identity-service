using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using ZivraFramework.EFCore.Context;
using ZivraFramework.EFCore.Repositories;

namespace IdentityService.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(IdentityDbContext ctx) : base(ctx) { }

    #region GetByUsernameAsync
    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        return await _set.FirstOrDefaultAsync(u => u.Username == username, ct);
    }
    #endregion

    #region GetByEmailAsync
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _set.FirstOrDefaultAsync(u => u.Email == email, ct);
    }
    #endregion

    #region ExistsByEmailAsync
    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _set.AnyAsync(u => u.Email == email, ct);
    }
    #endregion

    #region ExistsUsernameAsync
    public Task<bool> ExistsUsernameAsync(string username, CancellationToken ct = default)
    {
        return _set.AnyAsync(u => u.Username == username, ct);
    }
    #endregion
}