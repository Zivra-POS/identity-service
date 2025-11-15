using IdentityService.Shared.DTOs.Request.User;
using IdentityService.Shared.DTOs.Response.User;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace IdentityService.Core.Interfaces.Services;

public interface IUserClaimService
{
    Task<IEnumerable<UserClaimResponse>> GetByUserIdAsync(Guid userId);
    Task<UserClaimResponse> GetByIdAsync(Guid id);
    Task<UserClaimResponse> CreateAsync(UserClaimRequest req);
    Task<UserClaimResponse> UpdateAsync(UserClaimRequest req);
    Task<string> DeleteAsync(Guid id);
    Task<UserClaimResponse> GetByHashedIdAsync(string hashedId);
}
