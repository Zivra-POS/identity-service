using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Infrastructure.Persistence;
using ZivraFramework.EFCore.Context;
using ZivraFramework.EFCore.Repositories;
using System;

namespace IdentityService.Infrastructure.Repositories;

public class UserTokenRepository : GenericRepository<UserToken>, IUserTokenRepository
{
    public UserTokenRepository(IdentityDbContext ctx) : base(ctx)
    {
    }

    public async Task<UserToken?> GetByNameAndValueAsync(string name, string value, CancellationToken ct = default)
    {
        return await _set.FirstOrDefaultAsync(t => t.Name == name && t.Value == value, ct);
    }

    public async Task DeleteAsync(UserToken token, CancellationToken ct = default)
    {
        _set.Remove(token);
        await Task.CompletedTask;
    }
    
    public async Task<bool> ExistByNameAndUserIdAsync(string name, Guid userId, CancellationToken ct = default)
    {
        return await _set.AnyAsync(t => t.Name == name && t.UserId == userId, ct);
    }
    
    public async Task DeleteByNameAndUserIdAsync(string name, Guid userId, CancellationToken ct = default)
    {
        var tokens = await _set.Where(t => t.Name == name && t.UserId == userId).ToListAsync(ct);
        _set.RemoveRange(tokens);
    }
}
