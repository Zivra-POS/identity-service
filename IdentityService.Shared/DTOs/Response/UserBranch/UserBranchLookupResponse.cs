using ZivraFramework.Core.Models;

namespace IdentityService.Shared.DTOs.Response.UserBranch;

public class UserBranchLookupResponse : BaseDto
{
    public Guid UserId { get; set; }
    public Guid BranchId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public string? BranchName { get; set; }
    public string? BranchCode { get; set; }
    public bool IsPrimary { get; set; }
}
