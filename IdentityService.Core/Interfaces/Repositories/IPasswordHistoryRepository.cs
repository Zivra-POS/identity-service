using IdentityService.Core.Entities;
using ZivraFramework.Core.Interfaces;

namespace IdentityService.Core.Interfaces.Repositories;

public interface IPasswordHistoryRepository : IGenericRepository<PasswordHistory>
{
    Task<PasswordHistory?> GetRowByPasswordHashAsync(string passwordHash, CancellationToken cancellationToken = default);
    Task<List<PasswordHistory>> GetRowsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

