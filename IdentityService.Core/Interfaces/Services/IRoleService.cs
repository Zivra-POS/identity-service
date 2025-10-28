using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityService.Shared.DTOs.Request.Role;
using IdentityService.Shared.DTOs.Response.Role;
using IdentityService.Shared.Response;
using ZivraFramework.Core.Models;

namespace IdentityService.Core.Interfaces.Services;

public interface IRoleService
{
    Task<Result<IEnumerable<RoleResponse>>> GetAllAsync(PagedQuery query);
    Task<Result<RoleResponse>> GetByIdAsync(Guid id);
    Task<Result<RoleResponse>> CreateAsync(RoleRequest req);
    Task<Result<RoleResponse>> UpdateAsync(RoleRequest req);
    Task<Result<string>> DeleteAsync(Guid id);
}
