using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Shared.DTOs.User;
using Microsoft.EntityFrameworkCore;
using ZivraFramework.Core.Filtering;
using ZivraFramework.Core.Filtering.Entities;
using ZivraFramework.Core.Models;
using ZivraFramework.EFCore.Repositories;

namespace IdentityService.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    private readonly IdentityDbContext _ctx;

    public UserRepository(IdentityDbContext ctx) : base(ctx) { _ctx = ctx; }

    #region GetByUsernameAsync
    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        var result = await (from u in _set
                join s in _ctx.Stores on u.StoreId equals s.Id into storeJoin
                from s in storeJoin.DefaultIfEmpty()
                where u.Username == username
                select new { User = u, HashedStoreId = s != null ? s.HashedId : null })
            .FirstOrDefaultAsync(ct);

        if (result == null) return null;

        result.User.HashedStoreId = result.HashedStoreId;
        return result.User;
    }
    #endregion
    
    #region GetByIdWithRolesAsync
    public async Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _set
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Include(u => u.Claims)
            .Include(u => u.RefreshTokens)
            .Include(u => u.UserBranches)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

        if (user == null) return null;

        // Populate HashedStoreId if StoreId exists
        if (user.StoreId.HasValue)
        {
            var store = await _ctx.Stores.AsNoTracking().FirstOrDefaultAsync(s => s.Id == user.StoreId.Value, ct);
            user.HashedStoreId = store?.HashedId;
        }

        return user;
    }
    #endregion

    #region GetByEmailAsync
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalizedEmail = email?.ToUpperInvariant();

        var result = await (from u in _set
                            join s in _ctx.Stores on u.StoreId equals s.Id into storeJoin
                            from s in storeJoin.DefaultIfEmpty()
                            where u.NormalizedEmail == normalizedEmail
                            select new { User = u, HashedStoreId = s != null ? s.HashedId : null })
                           .FirstOrDefaultAsync(ct);

        if (result == null) return null;

        result.User.HashedStoreId = result.HashedStoreId;
        return result.User;
    }
    #endregion

    #region ExistsByEmailAsync
    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _set.AnyAsync(u => u.Email == email, ct);
    }
    #endregion

    #region ExistsUsernameAsync
    public Task<bool> ExistsUsernameAsync(string username, CancellationToken ct = default)
    {
        return _set.AnyAsync(u => u.Username == username, ct);
    }
    #endregion

    #region GetUserWithRolesAsync
    public async Task<UserWithRolesDto?> GetUserWithRolesAsync(Guid? userId, CancellationToken ct = default)
    {
        var result = await _set
            .Where(u => u.Id == userId)
            .Select(u => new UserWithRolesDto
            {
                Id = u.Id,
                Username = u.Username,
                FullName = u.FullName,
                RoleNames = u.UserRoles
                    .Select(ur => ur.Role!.Name)
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);

        return result;
    }
    #endregion
    
    #region UpdateStoreIdAsync
    public async Task UpdateStoreIdAsync(Guid userId, Guid storeId, CancellationToken ct = default)
    {
        await _set.Where(u => u.Id == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.StoreId, storeId), ct);
    }
    #endregion
    
    #region UpdateUserWithRolesAsync
    public async Task<bool> IsUserAccessStoreAsync(Guid userId, Guid storeId, CancellationToken ct = default)
    {
        return await _set.AnyAsync(u => u.Id == userId && u.StoreId == storeId, ct);
    }
    #endregion

    #region GetByHashedIdAsync
    public async Task<User?> GetByHashedIdAsync(string hashedId, CancellationToken ct = default)
    {
        return await _set.FirstOrDefaultAsync(u => u.HashedId == hashedId, ct);
    }
    #endregion
    
    #region GetStaffByStoreIdAsync
    public async Task<PagedResult<User>> GetStaffByStoreIdAsync(QueryRequest req, CancellationToken ct = default)
    {
        var q = _set.AsNoTracking().ApplyFiltering(req);

        var count = await q.CountAsync(ct);

        var users = await q
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .ToListAsync(ct);

        return new PagedResult<User>()
        {
            Items = users,
            TotalCount = count,
        };
    }
    #endregion
}