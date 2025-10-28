using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Infrastructure.Persistence;
using ZivraFramework.EFCore.Context;
using ZivraFramework.EFCore.Repositories;

namespace IdentityService.Infrastructure.Repositories;

public class UserSessionRepository : GenericRepository<UserSession>, IUserSessionRepository
{
    public UserSessionRepository(IdentityDbContext ctx) : base(ctx)
    {
    }

    public async Task<UserSession?> GetBySessionIdAsync(string sessionId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId)) return null;
        return await _set.FirstOrDefaultAsync(s => s.SessionId == sessionId, ct);
    }

    public async Task<IEnumerable<UserSession>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _set.AsNoTracking().Where(s => s.UserId == userId).ToListAsync(ct);
    }

    public async Task RevokeAsync(UserSession session, CancellationToken ct = default)
    {
        if (session == null) return;
        session.RevokedAt = DateTime.UtcNow;
        session.ModDate = DateTime.UtcNow;
        _set.Update(session);
        await Task.CompletedTask;
    }
}
