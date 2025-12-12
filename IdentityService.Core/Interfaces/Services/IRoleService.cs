using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityService.Shared.DTOs.Request.Role;
using IdentityService.Shared.DTOs.Response.Role;
using ZivraFramework.Core.Filtering.Entities;
using ZivraFramework.Core.Models;

namespace IdentityService.Core.Interfaces.Services;

public interface IRoleService
{
    Task<PagedResult<RoleResponse>> GetAllAsync(QueryRequest query);
    Task<RoleResponse> GetByIdAsync(Guid id);
    Task<RoleResponse> CreateAsync(RoleRequest req);
    Task<RoleResponse> UpdateAsync(RoleRequest req);
    Task<string> DeleteAsync(Guid id);
    Task<RoleResponse> GetByHashedIdAsync(string hashedId);
}
