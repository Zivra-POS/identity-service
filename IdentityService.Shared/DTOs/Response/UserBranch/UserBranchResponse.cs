namespace IdentityService.Shared.DTOs.Response.UserBranch;

public class UserBranchResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid BranchId { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime CreDate { get; set; }
    public string? CreBy { get; set; }
    public string? CreIpAddress { get; set; }
    public DateTime? ModDate { get; set; }
    public string? ModBy { get; set; }
    public string? ModIpAddress { get; set; }
    
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public string? BranchName { get; set; }
    public string? BranchCode { get; set; }
}
