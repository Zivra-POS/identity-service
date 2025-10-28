namespace IdentityService.Shared.DTOs.Response.Auth;

public class RegisterStaffResponse
{
    public Guid Id { get; set; }
    public required string FullName { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ProfileUrl { get; set; }
    public bool IsActive { get; set; }
    public Guid? StoreId { get; set; }
    public Guid? OwnerId { get; set; }

    // Address fields
    public string? Address { get; set; }
    public string? Province { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Rt { get; set; }
    public string? Rw { get; set; }

    public string[] Roles { get; set; } = Array.Empty<string>();
    
    public DateTime CreDate { get; set; }
    public string? CreBy { get; set; }
    public string? CreIpAddress { get; set; }
}