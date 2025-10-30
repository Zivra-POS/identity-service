using IdentityService.Shared.DTOs.Response.UserBranch;

namespace IdentityService.Core.Mappers.UserBranch;

public static class UserBranchMapper
{
    public static UserBranchResponse ToResponse(Entities.UserBranch ub)
    {
        return new UserBranchResponse
        {
            Id = ub.Id,
            UserId = ub.UserId,
            BranchId = ub.BranchId,
            IsPrimary = ub.IsPrimary,
            CreDate = ub.CreDate,
            CreBy = ub.CreBy,
            CreIpAddress = ub.CreIpAddress,
            ModDate = ub.ModDate,
            ModBy = ub.ModBy,
            ModIpAddress = ub.ModIpAddress,
            UserName = ub.User?.Username,
            UserEmail = ub.User?.Email,
            BranchName = ub.Branch?.Name,
            BranchCode = ub.Branch?.Code
        };
    }

    public static UserBranchLookupResponse ToLookupResponse(Entities.UserBranch ub)
    {
        return new UserBranchLookupResponse
        {
            Id = ub.Id,
            UserId = ub.UserId,
            BranchId = ub.BranchId,
            UserName = ub.User?.Username,
            UserEmail = ub.User?.Email,
            BranchName = ub.Branch?.Name,
            BranchCode = ub.Branch?.Code,
            IsPrimary = ub.IsPrimary
        };
    }
}
