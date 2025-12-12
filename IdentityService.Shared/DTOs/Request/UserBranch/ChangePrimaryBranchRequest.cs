using ZivraFramework.Core.Models;

namespace IdentityService.Shared.DTOs.Request.UserBranch;

public class ChangePrimaryBranchRequest : BaseDto
{
    public Guid UserId { get; set; }
    public Guid BranchId { get; set; }
    public bool IsPrimary { get; set; }
}