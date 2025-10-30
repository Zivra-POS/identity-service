using System.ComponentModel.DataAnnotations;

namespace IdentityService.Shared.DTOs.Request.UserBranch;

public class UserBranchRequest
{
    public Guid? Id { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public Guid BranchId { get; set; }
    
    public bool IsPrimary { get; set; }

    public DateTime CreDate { get; set; }
    public string? CreBy { get; set; }
    public string? CreIpAddress { get; set; }
}
