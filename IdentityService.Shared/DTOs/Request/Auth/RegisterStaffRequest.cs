using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace IdentityService.Shared.DTOs.Request.Auth;

public class RegisterStaffRequest
{
    public Guid? StoreId { get; set; }
    public Guid? OwnerId { get; set; }
    public required string FullName { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public IFormFile? ProfileImage { get; set; }

    [MaxLength(1000)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? Province { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? District { get; set; }

    [MaxLength(10)]
    public string? Rt { get; set; }

    [MaxLength(10)]
    public string? Rw { get; set; }

    public Guid[]? RoleIDs { get; set; }
    
    public DateTime CreDate { get; set; }
    public string? CreBy { get; set; }
    public string? CreIpAddress { get; set; }
}