using IdentityService.Shared.DTOs.Request.UserBranch;
using IdentityService.Shared.DTOs.Response.UserBranch;
using IdentityService.Shared.Response;
using ZivraFramework.Core.Models;

namespace IdentityService.Core.Interfaces.Services;

public interface IUserBranchService
{
    Task<Result<IEnumerable<UserBranchResponse>>> GetAllAsync(PagedQuery query, Guid storeId);
    Task<Result<UserBranchResponse>> GetByIdAsync(Guid id);
    Task<Result<IEnumerable<UserBranchResponse>>> GetByUserIdAsync(Guid userId);
    Task<Result<IEnumerable<UserBranchResponse>>> GetByBranchIdAsync(Guid branchId);
    Task<Result<UserBranchResponse>> CreateAsync(UserBranchRequest req);
    Task<Result<string>> DeleteAsync(Guid id);
    Task<Result<IEnumerable<UserBranchLookupResponse>>> GetRowsForLookupAsync(Guid storeId);
}
