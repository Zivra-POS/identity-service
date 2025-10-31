using IdentityService.Shared.Constants;

namespace IdentityService.Shared.DTOs.Request.Store;

public class StoreRequest
{
    public Guid? Id { get; set; }
    public Guid? UserId { get; set; } = default;
    public string Name { get; set; } = default!;
    public string? Address { get; set; }
    public string? Province { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Rt { get; set; }
    public string? Rw { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
    public CostingMethod CostingMethod { get; set; } = CostingMethod.FIFO;

    public DateTime CreDate { get; set; }
    public string? CreBy { get; set; }
    public string? CreIpAddress { get; set; }
}
