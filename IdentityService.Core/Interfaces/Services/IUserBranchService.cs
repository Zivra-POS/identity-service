using IdentityService.Shared.DTOs.Request.UserBranch;
using IdentityService.Shared.DTOs.Response.UserBranch;
using ZivraFramework.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace IdentityService.Core.Interfaces.Services;

public interface IUserBranchService
{
    Task<IEnumerable<UserBranchResponse>> GetAllAsync(PagedQuery query, Guid storeId);
    Task<UserBranchResponse> GetByIdAsync(Guid id);
    Task<IEnumerable<UserBranchResponse>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<UserBranchResponse>> GetByBranchIdAsync(Guid branchId);
    Task<UserBranchResponse> CreateAsync(UserBranchRequest req);
    Task<string> DeleteAsync(Guid id);
    Task<IEnumerable<UserBranchLookupResponse>> GetRowsForLookupAsync(Guid storeId);
    Task<UserBranchResponse> GetByHashedIdAsync(string hashedId);
}
