using IdentityService.Shared.DTOs.Request.Branch;
using IdentityService.Shared.DTOs.Response.Branch;
using IdentityService.Shared.Response;
using ZivraFramework.Core.Models;

namespace IdentityService.Core.Interfaces.Services;

public interface IBranchService
{
    Task<Result<IEnumerable<BranchResponse>>> GetAllAsync(PagedQuery query, Guid storeId);
    Task<Result<BranchResponse>> GetByIdAsync(Guid id);
    Task<Result<BranchResponse>> CreateAsync(BranchRequest req);
    Task<Result<BranchResponse>> UpdateAsync(BranchRequest req);
    Task<Result<string>> DeleteAsync(Guid id);
}

