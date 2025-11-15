using ZivraFramework.Core.Models;
using IdentityService.Shared.Constants;

namespace IdentityService.Shared.DTOs.Response.Store;

public class StoreResponse : BaseDto
{
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
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public CostingMethod CostingMethod { get; set; }
}
