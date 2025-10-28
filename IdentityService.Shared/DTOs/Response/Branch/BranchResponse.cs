namespace IdentityService.Shared.DTOs.Response.Branch;

public class BranchResponse
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; }
    public string Name { get; set; } = default!;
    public string? Code { get; set; }
    public string? Address { get; set; }
    public string? Province { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Rt { get; set; }
    public string? Rw { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; }

    public DateTime CreDate { get; set; }
    public string? CreBy { get; set; }
    public string? CreIpAddress { get; set; }
    public DateTime? ModDate { get; set; }
}

