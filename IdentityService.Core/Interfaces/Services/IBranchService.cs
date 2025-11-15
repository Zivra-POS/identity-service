using IdentityService.Shared.DTOs.Request.Branch;
using IdentityService.Shared.DTOs.Response.Branch;
using ZivraFramework.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using IdentityService.Core.Entities;
using ZivraFramework.Core.Filtering.Entities;

namespace IdentityService.Core.Interfaces.Services;

public interface IBranchService
{
    Task<IEnumerable<BranchResponse>> GetAllAsync(PagedQuery query, Guid storeId);
    Task<BranchResponse> GetByIdAsync(Guid id);
    Task<PagedResult<Branch>> GetALlByStoreId(QueryRequest query, string storeId);
    Task<BranchResponse> CreateAsync(BranchRequest req);
    Task<BranchResponse> UpdateAsync(BranchRequest req);
    Task<string> DeleteAsync(Guid id);
    Task<BranchResponse> GetByHashedIdAsync(string hashedId);
    Task<int> DeleteByListIdAsync(List<Guid> ids);
}
