using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IdentityService.Core.Entities;
using ZivraFramework.Core.Interfaces;

namespace IdentityService.Core.Interfaces.Repositories;

public interface IRoleRepository : IGenericRepository<Role>
{
    Task<IEnumerable<Role>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task<Role?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<Role?> GetByHashedIdAsync(string hashedId, CancellationToken ct = default);
}