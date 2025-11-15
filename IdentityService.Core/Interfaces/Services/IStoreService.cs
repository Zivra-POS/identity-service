using ZivraFramework.Core.Models;
using IdentityService.Shared.DTOs.Request.Store;
using IdentityService.Shared.DTOs.Response.Store;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace IdentityService.Core.Interfaces.Services
{
    public interface IStoreService
    {
        Task<IEnumerable<StoreResponse>> GetAllAsync(PagedQuery query);
        Task<StoreResponse> GetByIdAsync(Guid id);
        Task<StoreResponse> CreateAsync(StoreRequest req);
        Task<StoreResponse> UpdateAsync(StoreRequest req);
        Task<string> DeleteAsync(Guid id);
        Task<StoreResponse> GetByHashedIdAsync(string hashedId);
    }
}
