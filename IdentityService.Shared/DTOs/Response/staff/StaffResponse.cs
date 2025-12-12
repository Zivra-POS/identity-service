namespace IdentityService.Shared.DTOs.Response.staff;

public class StaffResponse
{
    public string HashedId { get; set; } = default!;
    public string? FullName { get; set; }
    public string Email { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public string? Province { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public List<StaffBranchResponse> Branches { get; set; } = new();
    public List<StaffRoleResponse> Roles { get; set; } = new();
}