using IdentityService.Core.Entities;
using ZivraFramework.Core.Interfaces;

namespace IdentityService.Core.Interfaces.Repositories;

public interface IUserRoleRepository : IGenericRepository<UserRole>
{
    Task<IEnumerable<UserRole>> GetRowsByUserIdAsync(Guid userId, CancellationToken ct = default);
}