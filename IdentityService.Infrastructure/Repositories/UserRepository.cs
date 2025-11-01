using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Shared.DTOs.User;
using Microsoft.EntityFrameworkCore;
using ZivraFramework.EFCore.Repositories;

namespace IdentityService.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(IdentityDbContext ctx) : base(ctx) { }

    #region GetByUsernameAsync
    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        return await _set.FirstOrDefaultAsync(u => u.Username == username, ct);
    }
    #endregion
    
    #region GetByIdWithRolesAsync
    public async Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken ct = default)
    {
        return await _set
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Include(u => u.Claims)
            .Include(u => u.RefreshTokens)
            .Include(u => u.UserBranches)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }
    #endregion


    #region GetByEmailAsync
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _set.FirstOrDefaultAsync(u => u.Email == email, ct);
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
}