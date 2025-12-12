using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Shared.DTOs.Response.UserRole;
using Microsoft.EntityFrameworkCore;
using ZivraFramework.Core.Filtering;
using ZivraFramework.Core.Filtering.Entities;
using ZivraFramework.Core.Models;
using ZivraFramework.EFCore.Repositories;

namespace IdentityService.Infrastructure.Repositories;

public class UserRoleRepository : GenericRepository<UserRole>, IUserRoleRepository
{
    public UserRoleRepository(IdentityDbContext ctx) : base(ctx)
    {
    }

    #region GetRowsByUserIdAsync
    public async Task<IEnumerable<UserRole>> GetRowsByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _set.Where(ur => ur.UserId == userId).ToListAsync(ct);
    }
    #endregion
    
    #region GetAllAsync
    public async Task<PagedResult<UserRoleResponse>> GetAllAsync(QueryRequest query, Guid userId, CancellationToken ct = default)
    {
        IQueryable<UserRole> q = _set.AsNoTracking()
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == userId)
            .ApplyFiltering(query);
        
        var total = await q.CountAsync(ct);
        
        var items = await q
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(ur => new UserRoleResponse
            {
                HashedId = ur.HashedId,
                Id = ur.Id,
                UserId = ur.UserId,
                RoleId = ur.RoleId,
                RoleName = ur.Role!.Name,
            })
            .ToListAsync(ct);
        
        return new PagedResult<UserRoleResponse>
        {
            TotalCount = total,
            Items = items
        };
    }
    #endregion
}