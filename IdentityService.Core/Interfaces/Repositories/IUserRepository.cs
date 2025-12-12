using IdentityService.Core.Entities;
using IdentityService.Shared.DTOs.Response.staff;
using ZivraFramework.Core.Interfaces;
using IdentityService.Shared.DTOs.User;
using ZivraFramework.Core.Filtering.Entities;
using ZivraFramework.Core.Models;

namespace IdentityService.Core.Interfaces.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsUsernameAsync(string username, CancellationToken ct = default);
    Task<UserWithRolesDto?> GetUserWithRolesAsync(Guid? userId, CancellationToken ct = default);
    Task UpdateStoreIdAsync(Guid userId, Guid storeId, CancellationToken ct = default);
    Task<bool> IsUserAccessStoreAsync(Guid userId, Guid storeId, CancellationToken ct = default);
    Task<User?> GetByHashedIdAsync(string hashedId, CancellationToken ct = default);
    Task<PagedResult<StaffResponse>> GetStaffByStoreIdAsync(QueryRequest req, Guid storeId, CancellationToken ct = default);
}