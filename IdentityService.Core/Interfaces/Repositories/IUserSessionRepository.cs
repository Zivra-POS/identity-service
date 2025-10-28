using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IdentityService.Core.Entities;
using ZivraFramework.Core.Interfaces;

namespace IdentityService.Core.Interfaces.Repositories;

public interface IUserSessionRepository : IGenericRepository<UserSession>
{
    Task<UserSession?> GetBySessionIdAsync(string sessionId, CancellationToken ct = default);
    Task<IEnumerable<UserSession>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task RevokeAsync(UserSession session, CancellationToken ct = default);
}
