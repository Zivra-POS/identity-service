using Microsoft.EntityFrameworkCore;
using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Infrastructure.Persistence;
using ZivraFramework.EFCore.Repositories;

namespace IdentityService.Infrastructure.Repositories;

public class UserTokenRepository : GenericRepository<UserToken>, IUserTokenRepository
{
    public UserTokenRepository(IdentityDbContext ctx) : base(ctx)
    {
    }

    #region GetByNameAndValueAsync
    public async Task<UserToken?> GetByNameAndValueAsync(string name, string value, CancellationToken ct = default)
    {
        return await _set
            .FirstOrDefaultAsync(t => t.Name == name && t.Value == value, ct);
    }
    #endregion

    #region DeleteAsync
    public async Task DeleteAsync(UserToken token, CancellationToken ct = default)
    {
        _set.Remove(token);
        await Task.CompletedTask;
    }
    #endregion

    #region ExistByNameAndUserIdAsync
    public async Task<bool> ExistByNameAndUserIdAsync(string name, Guid userId, CancellationToken ct = default)
    {
        return await _set.AnyAsync(t => t.Name == name && t.UserId == userId, ct);
    }
    #endregion

    #region DeleteByNameAndUserIdAsync
    public async Task DeleteByNameAndUserIdAsync(string name, Guid userId, CancellationToken ct = default)
    {
        var tokens = await _set
            .Where(t => t.Name == name && t.UserId == userId)
            .ToListAsync(ct);

        _set.RemoveRange(tokens);
    }
    #endregion
}