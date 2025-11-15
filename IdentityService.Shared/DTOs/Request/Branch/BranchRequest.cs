using ZivraFramework.Core.Models;

namespace IdentityService.Shared.DTOs.Request.Branch;

public class BranchRequest : BaseDto
{
    public string? StoreId { get; set; }
    public string Name { get; set; } = default!;
    public string? Code { get; set; }
    public string? Address { get; set; }
    public string? Province { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Rt { get; set; }
    public string? Rw { get; set; }
    public string? Phone { get; set; }
}
