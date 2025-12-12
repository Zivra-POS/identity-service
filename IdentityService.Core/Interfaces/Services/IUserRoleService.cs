using IdentityService.Shared.DTOs.Request.UserRole;
using IdentityService.Shared.DTOs.Response.UserRole;
using ZivraFramework.Core.Filtering.Entities;
using ZivraFramework.Core.Models;

namespace IdentityService.Core.Interfaces.Services;

public interface IUserRoleService
{
    Task<PagedResult<UserRoleResponse>> GetAllAsync(QueryRequest query, Guid userId);
    Task<int> CreateBulkAsync(CreateUserRoleRequest req);
    Task<int> DeleteBulkAsync(List<Guid> ids);
}