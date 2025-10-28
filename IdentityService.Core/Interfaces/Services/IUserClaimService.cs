using IdentityService.Shared.DTOs.UserClaim;
using IdentityService.Shared.Response;

namespace IdentityService.Core.Interfaces.Services;

public interface IUserClaimService
{
    Task<Result<IEnumerable<UserClaimResponse>>> GetByUserIdAsync(Guid userId);
    Task<Result<UserClaimResponse>> GetByIdAsync(Guid id);
    Task<Result<UserClaimResponse>> CreateAsync(UserClaimRequest req);
    Task<Result<UserClaimResponse>> UpdateAsync(UserClaimRequest req);
    Task<Result<string>> DeleteAsync(Guid id);
}
