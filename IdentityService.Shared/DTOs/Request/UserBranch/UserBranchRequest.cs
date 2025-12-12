using System.ComponentModel.DataAnnotations;
using ZivraFramework.Core.Models;

namespace IdentityService.Shared.DTOs.Request.UserBranch;

public class UserBranchRequest : BaseDto
{
    [Required]
    public string? UserId { get; set; }

    [Required]
    public Guid BranchId { get; set; }
    public string? BranchName { get; set; }

    public bool IsPrimary { get; set; }
}
