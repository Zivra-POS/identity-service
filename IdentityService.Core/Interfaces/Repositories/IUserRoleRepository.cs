using IdentityService.Core.Entities;
using IdentityService.Shared.DTOs.Response.UserRole;
using ZivraFramework.Core.Filtering.Entities;
using ZivraFramework.Core.Interfaces;
using ZivraFramework.Core.Models;

namespace IdentityService.Core.Interfaces.Repositories;

public interface IUserRoleRepository : IGenericRepository<UserRole>
{
    Task<IEnumerable<UserRole>> GetRowsByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<PagedResult<UserRoleResponse>> GetAllAsync(QueryRequest query, Guid userId, CancellationToken ct = default);
}