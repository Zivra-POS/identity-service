using IdentityService.Shared.DTOs.Request.Store;
using IdentityService.Shared.DTOs.Response.Store;
using IdentityService.Shared.Response;
using ZivraFramework.Core.Models;

namespace IdentityService.Core.Interfaces.Services;

public interface IStoreService
{
    Task<Result<IEnumerable<StoreResponse>>> GetAllAsync(PagedQuery query);
    Task<Result<StoreResponse>> GetByIdAsync(Guid id);
    Task<Result<StoreResponse>> CreateAsync(StoreRequest req);
    Task<Result<StoreResponse>> UpdateAsync(StoreRequest req);
    Task<Result<string>> DeleteAsync(Guid id);
}
