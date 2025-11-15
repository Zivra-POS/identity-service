using ZivraFramework.Core.Models;

namespace IdentityService.Shared.DTOs.Response.Auth;

public class UpdateUserResponse : BaseDto
{
    public required string FullName { get; set; }
    public string Username { get; set; } = default!;
    public string? DisplayName { get; set; }
    public string Email { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    public string? ProfileUrl { get; set; }
    public bool IsActive { get; set; }
    public string? Address { get; set; }
    public string? Province { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Rt { get; set; }
    public string? Rw { get; set; }


    public IEnumerable<string>? Roles { get; set; }
}