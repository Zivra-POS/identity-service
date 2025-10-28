using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace IdentityService.Shared.DTOs.Request.Auth;

public class UpdateUserRequest
{
    public Guid Id { get; set; }
    public required string FullName { get; set; }
    public string? DisplayName { get; set; }
    public string? PhoneNumber { get; set; }
    public bool? IsActive { get; set; }
    public IFormFile? ProfilImage { get; set; }

    // Address fields
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

    public DateTime? ModDate { get; set; }
    public string? ModBy { get; set; }
    public string? ModIpAddress { get; set; }
}