using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Infrastructure.Persistence;
using ZivraFramework.EFCore.Context;
using ZivraFramework.EFCore.Repositories;

namespace IdentityService.Infrastructure.Repositories;

public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(IdentityDbContext ctx) : base(ctx)
    {
    }
    
    #region GetByUserIdAndTokenHashAsync
    public async Task<RefreshToken?> GetByUserIdAndTokenHashAsync(Guid userId, string tokenHash, CancellationToken ct = default)
    {
        return await _set.FirstOrDefaultAsync(x => x.UserId == userId
                                                   && x.Token == tokenHash
                                                   && x.Revoked == null
                                                   && x.Expires > DateTime.UtcNow, ct);
    }
    #endregion
    
    #region GetByTokenHashAsync
    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default)
    {
        return await _set
            .Include(rt => rt.User!)
                .ThenInclude(u => u.UserRoles!)
                    .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(x => x.Token == tokenHash && x.Revoked == null && x.Expires > DateTime.UtcNow, ct);
    }
    #endregion
    
    #region RevokeAsync
    public async Task RevokeAsync(RefreshToken token, CancellationToken ct = default)
    {
        token.Revoked = DateTime.UtcNow;
        token.RevokedByIp = token.RevokedByIp; 
        token.ModDate = DateTime.UtcNow;

        _set.Update(token);
        await Task.CompletedTask;
    }
    #endregion

    #region GetActiveByUserIdAsync
    public async Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _set
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Revoked == null && x.Expires > DateTime.UtcNow)
            .ToListAsync(ct);
    }
    #endregion
    
    #region GetByDeviceIdAsync
    public async Task<IEnumerable<RefreshToken>> GetByDeviceIdAsync(string deviceId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(deviceId)) return Array.Empty<RefreshToken>();

        return await _set
            .AsNoTracking()
            .Where(x => x.DeviceId == deviceId && x.Revoked == null && x.Expires > DateTime.UtcNow)
            .ToListAsync(ct);
    }
    #endregion
}
