using System;
using System.Threading;
using System.Threading.Tasks;
using IdentityService.Core.Entities;
using ZivraFramework.Core.Interfaces;

namespace IdentityService.Core.Interfaces.Repositories;

public interface IUserTokenRepository : IGenericRepository<UserToken>
{
    Task<UserToken?> GetByNameAndValueAsync(string name, string value, CancellationToken ct = default);
    Task DeleteAsync(UserToken token, CancellationToken ct = default);
    Task<bool> ExistByNameAndUserIdAsync(string name, Guid userId, CancellationToken ct = default);
    Task DeleteByNameAndUserIdAsync(string name, Guid userId, CancellationToken ct = default);
}
