using System.Threading;
using System.Threading.Tasks;
using IdentityService.Core.Entities;
using ZivraFramework.Core.Interfaces;

namespace IdentityService.Core.Interfaces.Repositories;

public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    Task<RefreshToken?> GetByUserIdAndTokenHashAsync(Guid userId, string tokenHash, CancellationToken ct = default);
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);
    Task RevokeAsync(RefreshToken token, CancellationToken ct = default);

    // Added: fetch active refresh tokens for a user (used by logout)
    Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default);

    // Added: fetch refresh tokens by device id / session id
    Task<IEnumerable<RefreshToken>> GetByDeviceIdAsync(string deviceId, CancellationToken ct = default);
}
