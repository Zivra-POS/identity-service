using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IdentityService.Core.Entities;
using IdentityService.Shared.DTOs.Response.Role;
using ZivraFramework.Core.Filtering.Entities;
using ZivraFramework.Core.Interfaces;
using ZivraFramework.Core.Models;

namespace IdentityService.Core.Interfaces.Repositories;

public interface IRoleRepository : IGenericRepository<Role>
{
    Task<PagedResult<RoleResponse>> GetAllAsync(QueryRequest query, CancellationToken ct = default);
    Task<IEnumerable<Role>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task<Role?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<Role?> GetByHashedIdAsync(string hashedId, CancellationToken ct = default);
}